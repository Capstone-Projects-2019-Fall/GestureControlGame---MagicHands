# import the necessary packages
from imutils.video import VideoStream
from imutils.video import FPS
import argparse
import imutils
import time
import cv2
import socket
import time
import helpers
import math
import multiprocessing


def tracker(cv_name, conn):
    tracker = helpers.get_tracker(cv_name)
    count = 0
    while True:
        signal = conn.recv()
        # print("signal received")
        if isinstance(signal, list) and signal[0] == "init":
            frame, initBB = signal[1:]
            tracker.init(frame, initBB)
            # print("tracker initialized")
        else:
            count += 1
            # print(f"received frame #{count}")
            frame = signal
            result = tracker.update(frame)
            conn.send(result)


if __name__ == "__main__":

    UDP_IP = "127.0.0.1"
    UDP_PORT = 5065

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    # construct the argument parser and parse the arguments
    ap = argparse.ArgumentParser()
    ap.add_argument("-v", "--video", type=str,
        help="path to input video file")
    ap.add_argument("-t", "--tracker", type=str, default="csrt",
        help="OpenCV object tracker type")
    args = vars(ap.parse_args())

    main_conn_1, left_conn = multiprocessing.Pipe()
    main_conn_2, right_conn = multiprocessing.Pipe()
    left_proc = multiprocessing.Process(target=tracker, args=(args["tracker"], left_conn))
    right_proc = multiprocessing.Process(target=tracker, args=(args["tracker"], right_conn))
    left_proc.daemon = 1
    right_proc.daemon = 1
    left_proc.start()
    right_proc.start()

    # tracker_left = helpers.get_tracker(args["tracker"])
    # tracker_right = helpers.get_tracker(args["tracker"])

    # initialize the bounding box coordinates of the object we are going
    # to track
    initBB_left = None
    initBB_right = None

    # if a video path was not supplied, grab the reference to the web cam
    if not args.get("video", False):
        print("[INFO] starting video stream...")
        vs = VideoStream(src=0).start()
        time.sleep(1.0)

    # otherwise, grab a reference to the video file
    else:
        vs = cv2.VideoCapture(args["video"])

    # initialize the FPS throughput estimator
    fps = None

    # loop over frames from the video stream
    calibrating = False
    cal_start = None
    cal_time = 5

    w, h = 100, 100
    # x1, y1 = 50, 200
    # x2, y2 = 300, 200




    while True:
        # grab the current frame, then handle if we are using a
        # VideoStream or VideoCapture object
        frame = vs.read()
        frame = frame[1] if args.get("video", False) else frame


        # check to see if we have reached the end of the stream
        if frame is None:
            break


        # resize the frame (so we can process it faster) and grab the
        # frame dimensionsq
        frame = imutils.resize(frame, width=500)
        (H, W) = frame.shape[:2]
        w, h = W//6, W//6
        x1, y1 = W//6, H//3
        x2, y2 = 4*W//6, H//3

        frame = cv2.flip(frame, 1)

        # check to see if we are currently tracking an object
        if initBB_left is not None and initBB_right is not None:

            # grab the new bounding box coordinates of the object
            # (success_left, box_left) = tracker_left.update(frame)
            # (success_right, box_right) = tracker_right.update(frame)
            main_conn_1.send(frame)
            main_conn_2.send(frame)

            (success_left, box_left) = main_conn_1.recv()
            (success_right, box_right) = main_conn_2.recv()

            to_send = []
            fail_message = "x x"
            # check to see if the tracking was a success
            if success_left:
                (x, y, w, h) = [int(v) for v in box_left]
                cv2.rectangle(frame, (x, y), (x + w, y + h),(0, 255, 0), 2)
                xp = (x + w//2 - W//2)/ W
                yp = - (y + h//2 - H//2) / H
                to_send.append(f"{xp} {yp}")
            else:
                to_send.append(fail_message)

            if success_right:
                (x, y, w, h) = [int(v) for v in box_right]
                cv2.rectangle(frame, (x, y), (x + w, y + h),(0, 255, 0), 2)
                xp = (x + w//2 - W//2) / W
                yp = - (y + h//2 - H//2) / H
                to_send.append(f"{xp} {yp}")
            else:
                to_send.append(fail_message)

            sock.sendto(",".join(to_send).encode(), (UDP_IP, UDP_PORT))
            # update the FPS counter
            fps.update()
            fps.stop()

            # initialize the set of information we'll be displaying on
            # the frame
            info = [
                ("Tracker", args["tracker"]),
                ("Success", "Yes" if success_left else "No"),
                ("FPS", "{:.2f}".format(fps.fps())),
            ]

            # loop over the info tuples and draw them on our frame
            for (i, (k, v)) in enumerate(info):
                text = "{}: {}".format(k, v)
                cv2.putText(frame, text, (10, H - ((i * 20) + 20)),
                            cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)



        # if the 's' key is selected, we are going to "select" a bounding
        # box to track

        if calibrating:
            # draw two bounding box so that you can put your fist into it.

            cv2.rectangle(frame, (x1, y1), (x1 + w, y1 + h), (0, 255, 0), 2)
            cv2.rectangle(frame, (x2, y2), (x2 + w, y2 + h), (0, 255, 0), 2)

            # count down from 5
            sec = cal_time - (time.time() - cal_start)
            cv2.putText(frame, str(math.ceil(sec)), (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)

            if sec < 0:
                calibrating = False
                # start tracking those boxes
                initBB_left = (x1, y1, w, h)
                initBB_right = (x2, y2, w, h)
                main_conn_1.send(["init", frame, initBB_left])
                main_conn_2.send(["init", frame, initBB_right])
                # tracker_left.init(frame, initBB_left)
                # tracker_right.init(frame, initBB_right)
                fps = FPS().start()

        # show the output frame
        cv2.imshow("Frame", frame)
        key = cv2.waitKey(1) & 0xFF

        if key == ord("s"):
            calibrating = True
            cal_start = time.time()

        # if the `q` key was pressed, break from the loop
        elif key == ord("q"):
            break

    # if we are using a webcam, release the pointer
    if not args.get("video", False):
        vs.stop()

    # otherwise, release the file pointer
    else:
        vs.release()

    # close all windows
    cv2.destroyAllWindows()