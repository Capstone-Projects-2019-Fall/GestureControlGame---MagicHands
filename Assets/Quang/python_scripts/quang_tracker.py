from imutils.video import VideoStream
from imutils.video import FPS
import argparse
import imutils
import time
import cv2
import socket
import time
import math
import numpy as np
import collections

def get_fore_ground_mask(frame, background, offset):
    a = np.any(np.greater(frame, background + offset), axis=-1)
    b = np.any(np.less(frame, background - offset), axis=-1)
    return np.logical_or(a, b).astype(np.uint8)

def get_foreground_mask_channel(frame, background, channel, offset):
    greater = np.greater(frame[:,:,channel], background[:,:,channel] + offset)
    less = np.less(frame[:,:,channel], background[:,:,channel] - offset)
    return np.logical_or(greater, less)

def get_foreground_mask_hsv(frame, background, offset):
    mask = []
    for channel in range(3):
        mask.append(get_foreground_mask_channel(frame, background, channel, offset[channel]))
    mask = np.stack(mask)
    # h_mask = get_foreground_mask_channel(frame, background, 0, offset[0])
    # s_mask = get_foreground_mask_channel(frame, background, 1, offset[1])
    # v_mask = get_foreground_mask_channel(frame, background, 2, offset[2])
    return np.any(mask,0)

HSV_OFFSET_HIGH = np.array([30,80,255])
HSV_OFFSET_LOW = np.array([30,80,255])
BACKGROUND_OFFSET = np.array([3,0,0])

vs = VideoStream(src=0).start()

is_first = True
fps_started = False
start_time = time.time()
backSub = cv2.createBackgroundSubtractorMOG2()
first_frame = None
first_grey_frame = None
prev_frame = None
background = None
background_hsv = None
sampling_color = False
sampling_start = None
sampling_time = 5 #seconds
fps = None
started = False
left_sample = None
right_sample = None
hsv_mean_left = None
hsv_mean_right = None
hsv_low_left = None
hsv_high_left = None
hsv_low_right = None
hsv_high_right = None
current_face = None
num_frame_with_no_face = 0
WIDTH = 500
fist_points = []
mask_trail = collections.deque()
MAX_TRAIL_LEN = 2


erode_kernel = np.ones((8, 8), np.uint8)
dilate_kernel = np.ones((30,30), np.uint8)

# face_cascade = cv2.CascadeClassifier("haarcascade_frontalface_default.xml")
face_cascade = cv2.CascadeClassifier(cv2.data.haarcascades + "haarcascade_frontalface_default.xml")

def get_blob_detector():
    # Setup SimpleBlobDetector parameters.
    params = cv2.SimpleBlobDetector_Params()

    # # Change thresholds
    # params.minThreshold = 10
    # params.maxThreshold = 200

    # # Filter by Area.
    # params.filterByArea = True
    # params.minArea = 1500
    #
    # Filter by Circularity
    params.filterByCircularity = True
    params.minCircularity = 0.1
    #
    # # Filter by Convexity
    # params.filterByConvexity = True
    # params.minConvexity = 0.87
    #
    # Filter by Inertia
    params.filterByInertia = True
    params.minInertiaRatio = 0.01

    # Create a detector with the parameters
    ver = (cv2.__version__).split('.')
    if int(ver[0]) < 3:
        detector = cv2.SimpleBlobDetector(params)
    else:
        detector = cv2.SimpleBlobDetector_create(params)
    return detector

def remove_flickering(trail):
    ret = np.all(np.stack(trail), axis=0).astype(np.uint8)
    return ret


def stat_to_normal(stat):
    return stat[cv2.CC_STAT_LEFT], stat[cv2.CC_STAT_TOP], stat[cv2.CC_STAT_WIDTH], stat[cv2.CC_STAT_HEIGHT], stat[cv2.CC_STAT_AREA]

def get_fist_point(mask, stat, n=10, p=0.8):
    forward = 0
    backward = 0
    x,y,w,h,a = stat_to_normal(stat)
    h, w = h-1, w-1
    x_step = w/n
    y_step = h/n
    for i in range(n):
        forward += mask[int(y+h - i*y_step), int(x + i*x_step)]
        backward += mask[int(y + i*y_step), int(x+ i*x_step)]
    if forward > backward:
        return int(x + p*w), int(y+(1-p)*h)
    else:
        return int(x+(1-p)*w), int(y+(1-p)*h)

def get_hsv_mask(frame, low, high):
    frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    # print(frame.shape)
    h_mask1 = np.greater(frame[:,:,0], low[0])
    h_mask2 = np.less(frame[:,:,0], high[0])
    h_mask = np.logical_and(h_mask1, h_mask2)

    v_mask1 = np.greater(frame[:,:,1], low[1])
    v_mask2 = np.less(frame[:,:,1], high[1])
    v_mask = np.logical_and(v_mask1, v_mask2)

    return np.logical_and(h_mask, v_mask).astype(np.uint8)

def get_hsv_mean(sample):
    sample = cv2.cvtColor(sample, cv2.COLOR_BGR2HSV)
    mean = np.mean(np.mean(sample, axis=0), axis=0)
    mean = mean.astype(np.int32)
    print("hsv_mean:", mean)
    return mean

def get_hand_pos(ellipse):
    # ellipse = ((x,y), (2a, 2b), angle)
    (x,y), (A, B), angle = ellipse
    angle = 90 - angle
    if angle < 0:
        angle = angle + 180
    angle = angle / 180 * math.pi
    B = B*0.7
    x_ret = int(x + math.cos(angle) * B/2)
    y_ret = int(y - math.sin(angle) * B/2)
    return x_ret, y_ret

def remove_black_bars(frame):
    h = frame.shape[0]
    n = 8
    frame = frame[h//n:(n-1)*h//n]
    return frame

def get_face_mask(faces, frame):
    mask = np.mean(frame, axis=-1) * 0
    for (x, y, w, h) in faces:
        # To draw a rectangle in a face
        cv2.rectangle(mask, (x, int(y-0.15*h)), (x + w, y + int(h * 1.4)), (255, 255, 255), -1)
    mask = (1 - mask // 254).astype(np.uint8)
    return mask

def get_biggest_face(faces):
    if len(faces) == 0:
        return None
    area = 0
    return_index = 0
    try:
        for i,(x,y,w,h) in enumerate(faces):
            if w*h > area:
                area = w*h
                return_index = i
    except:
        print(faces)
        print(current_face)
        exit()
    return return_index

def get_faces(faces, current_face):
    if len(faces) > 0:
        return faces

    if current_face is None:
        return []

    return [current_face]

def remove_small_regions(mask, min_area):
    connectivity = 4
    num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(mask, connectivity, cv2.CV_32S)
    ret = np.zeros((labels.shape), np.uint8)
    # for every component in the image, you keep it only if it's above min_size
    for i in range(1, num_labels):
        area = stats[i, cv2.CC_STAT_AREA]
        if area >= min_area:
            ret[labels == i] = 1
    return ret


detector = get_blob_detector()
# detector = cv2.SimpleBlobDetector()

while True:
    frame = vs.read()
    frame = imutils.resize(frame, width=WIDTH)
    frame = cv2.flip(frame, 1)
    frame = remove_black_bars(frame)
    print("frame first:", frame.shape)
    grey_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    hsv_frame = frame
    pure = frame.copy()
    # hsv_frame = cv2.cvtColor(frame, cv2.COLOR_RGB2HSV)


    H, W = frame.shape[:2]
    w, h = W // 16, W // 16
    x1, y1 = W // 6, H // 3
    x2, y2 = 4 * W // 6, H // 3

    if is_first:
        is_first = False
        first_frame = frame
        first_grey_frame = cv2.cvtColor(first_frame, cv2.COLOR_BGR2GRAY)
        prev_frame = frame

    # getting hands' color
    if sampling_color:
        # count down from 5
        sec = sampling_time - (time.time() - sampling_start)
        cv2.putText(frame, str(math.ceil(sec)), (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)

        if sec < 0:
            sampling_color = False
            started = True
            initBB_left = (x1, y1, w, h)
            initBB_right = (x2, y2, w, h)
            left_sample = frame[y1:y1+h, x1:x1+w]
            right_sample = frame[y2:y2+h, x2:x2+w]
            cv2.imwrite("left.jpg",left_sample)
            cv2.imwrite("right.jpg", right_sample)
            hsv_mean_left = get_hsv_mean(left_sample)
            hsv_mean_right = get_hsv_mean(right_sample)
            hsv_low_left = hsv_mean_left - HSV_OFFSET_LOW
            hsv_high_left = hsv_mean_left + HSV_OFFSET_HIGH

            hsv_low_right = hsv_mean_right - HSV_OFFSET_LOW
            hsv_high_right = hsv_mean_right + HSV_OFFSET_HIGH

            fps = FPS().start()



        # draw two bounding box so that you can put your fist into it.
        cv2.rectangle(frame, (x1, y1), (x1 + w, y1 + h), (0, 255, 0), 2)
        cv2.rectangle(frame, (x2, y2), (x2 + w, y2 + h), (0, 255, 0), 2)


    if background is not None and hsv_mean_left is not None and hsv_mean_right is not None:
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        faces = face_cascade.detectMultiScale(gray, 1.3, 5)
        if len(faces) == 0:
            num_frame_with_no_face += 1
        else:
            num_frame_with_no_face = 0
        if num_frame_with_no_face > 5:
            current_face = None
        faces = get_faces(faces, current_face)
        biggest_face = get_biggest_face(faces)
        if biggest_face is not None:
            current_face = faces[biggest_face]


        frame = cv2.GaussianBlur(frame, (5, 5), 0)
        # mask = backSub.apply(frame) // 224


        fg_mask = get_fore_ground_mask(frame, background, 20)
        # fg_mask = get_foreground_mask_hsv(hsv_frame, background_hsv, BACKGROUND_OFFSET)

        hsv_mask_left = get_hsv_mask(frame, hsv_low_left, hsv_high_left)
        hsv_mask_right = get_hsv_mask(frame, hsv_low_right, hsv_high_right)
        hsv_mask = np.logical_or(hsv_mask_left, hsv_mask_right).astype(np.uint8)

        face_mask = get_face_mask(faces, frame)

        mask = hsv_mask * fg_mask * face_mask
        mask = cv2.erode(mask, erode_kernel, iterations=1)
        # mask = remove_small_regions(mask, min_area=5)
        mask_trail.append(mask)
        if len(mask_trail) > MAX_TRAIL_LEN:
            mask_trail.popleft()
        mask = remove_flickering(mask_trail)


        mask = cv2.dilate(mask, dilate_kernel, iterations=1)
        mask_img = (mask * 224).astype(np.uint8)
        print("mask:",mask.shape)

        connectivity = 4
        num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(mask, connectivity, cv2.CV_32S)

        stats = list(stats[1:])
        if len(stats) >= 2:
            stats = sorted(stats, key= lambda x: -x[cv2.CC_STAT_AREA])
            fist_points = []
            for stat in stats[:2]:
                if stat[cv2.CC_STAT_AREA] > 1000:
                    fist_points.append(get_fist_point(mask, stat))

        for p in fist_points:
            cv2.circle(mask_img, p, 5, (255,0,0), -1)
            cv2.circle(pure, p, 5, (0, 0, 255), -1)



        cnts, hierarchy = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        cnts = sorted(cnts, key=cv2.contourArea)



        # mask = get_fore_ground_mask(frame, prev_frame, 10)

        frame = frame * np.expand_dims(mask, axis=-1)
        frame = frame.astype(np.uint8)

        keypoints = detector.detect(mask_img)
        print(len(keypoints))

        # cv2.drawContours(frame, cnts, -1, (0, 255, 0), 3)
        print("frame:", frame.shape)
        frame =  cv2.drawKeypoints(frame, keypoints, np.array([]), (0, 0, 255), cv2.DRAW_MATCHES_FLAGS_DRAW_RICH_KEYPOINTS)

        # for cnt in cnts[-2:]:
        #     if cv2.contourArea(cnt) < 1000:
        #         continue
        #     ellipse = cv2.fitEllipse(cnt)
        #     # print(ellipse)
        #     cv2.ellipse(frame, ellipse, (255, 0, 0), 2)
        #     hand_pos = get_hand_pos(ellipse)
        #     cv2.circle(frame, hand_pos, 8, (0,0,255),-1)



        # remove face


        # remove everything but hands





        prev_frame = frame

        fps.update()
        fps.stop()

        frame = mask_img

        cv2.putText(frame, "FPS: %.2f"%(fps.fps()), (10, H-20), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
        print("frame after text:",frame.shape)


    cv2.imshow("Frame", frame)
    cv2.imshow("Pure", pure)
    key = cv2.waitKey(1) & 0xFF

    if key == ord("q"):
        break
    elif key == ord("b"): # take background picture
        background = frame
        # background_hsv = cv2.cvtColor(background, cv2.COLOR_RGB2HSV)
        background_hsv = frame
    elif key == ord("h"): # get hands' color
        # give the user 5 secs to put their hands in the box
        sampling_color = True
        sampling_start = time.time()
