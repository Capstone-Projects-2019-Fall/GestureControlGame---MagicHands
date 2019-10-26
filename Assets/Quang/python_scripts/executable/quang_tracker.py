from imutils.video import VideoStream
from imutils.video import FPS
import imutils
import cv2
import socket
import time
import math
import numpy as np
import collections
import argparse

ap = argparse.ArgumentParser()
ap.add_argument("-F", "--face", type=str, default="haarcascade_frontalface_default.xml", help="path to face cascade file")
ap.add_argument("-H", "--hue", type=int, default=30, help="hue offset")
ap.add_argument("-S", "--saturation", type=int, default=90, help="saturation offset")
ap.add_argument("-V", "--value", type=int, default=100, help="value offset")
ap.add_argument("-B", "--background", type=int, default=10, help="background offset")
args = vars(ap.parse_args())

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

HSV_OFFSET = np.array([args["hue"],args["saturation"],args["value"]])
BACKGROUND_OFFSET = args["background"]




is_first = True
fps_started = False
start_time = time.time()
backSub = cv2.createBackgroundSubtractorMOG2()
first_frame = None
first_grey_frame = None
background = None
background_hsv = None
background_int32 = None
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
last_fistpoints = []
SMALL_WIDTH = 20
reverse = False

class Keys:
    def __init__(self):
        self.SAMPLE_HAND = "a"
        self.SAMPLE_BACKGROUND = "b"
        self.QUIT = "q"
        self.HUE = "h"
        self.SAT = "s"
        self.VAL = "v"
        self.REVERSE = "r"
        self.BACKGROUND_OFFSET = "o"

keys = Keys()

erode_kernel = np.ones((8, 8), np.uint8)
dilate_kernel = np.ones((30,30), np.uint8)

face_cascade = cv2.CascadeClassifier(args["face"])
def reset_cam(cam):
    # cam.set(cv2.CAP_PROP_BRIGHTNESS, 255//2)  # brightness     min: 0   , max: 255 , increment:1
    # cam.set(cv2.CAP_PROP_CONTRAST, 255//2)  # contrast       min: 0   , max: 255 , increment:1
    # cam.set(cv2.CAP_PROP_SATURATION, 255//2)  # saturation     min: 0   , max: 255 , increment:1
    # cam.set(cv2.CAP_PROP_HUE, 255//2)  # hue
    # cam.set(cv2.CAP_PROP_GAIN, 127//2)  # gain           min: 0   , max: 127 , increment:1
    cam.set(cv2.CAP_PROP_EXPOSURE, -5)  # exposure       min: -7  , max: -1  , increment:1
    # cam.set(cv2.CAP_PROP_WHITE_BALANCE_BLUE_U, 5500)  # white_balance  min: 4000, max: 7000, increment:1
    # cam.set(cv2.CAP_PROP_FOCUS, 10)  # focus          min: 0   , max: 255 , increment:5
    # cam.set(cv2.CAP_PROP_AUTO_EXPOSURE, 0.75)
    return

def reset_cam_0(cam):
    cam.set(cv2.CAP_PROP_BRIGHTNESS, cam.get(cv2.CAP_PROP_BRIGHTNESS))  # brightness     min: 0   , max: 255 , increment:1
    cam.set(cv2.CAP_PROP_CONTRAST, cam.get(cv2.CAP_PROP_CONTRAST))  # contrast       min: 0   , max: 255 , increment:1
    cam.set(cv2.CAP_PROP_SATURATION, 255)  # saturation     min: 0   , max: 255 , increment:1
    cam.set(cv2.CAP_PROP_HUE, cam.get(cv2.CAP_PROP_HUE))  # hue
    cam.set(cv2.CAP_PROP_GAIN, cam.get(cv2.CAP_PROP_GAIN))  # gain           min: 0   , max: 127 , increment:1
    cam.set(cv2.CAP_PROP_EXPOSURE, cam.get(cv2.CAP_PROP_EXPOSURE))  # exposure       min: -7  , max: -1  , increment:1
    cam.set(cv2.CAP_PROP_WHITE_BALANCE_BLUE_U, cam.get(cv2.CAP_PROP_WHITE_BALANCE_BLUE_U))  # white_balance  min: 4000, max: 7000, increment:1
    cam.set(cv2.CAP_PROP_FOCUS, cam.get(cv2.CAP_PROP_FOCUS))  # focus          min: 0   , max: 255 , increment:5

    return

def adjust_exposure(cam, target_brightness):
    cam.set(cv2.CAP_PROP_AUTO_EXPOSURE, 0.25)
    exposures = list(range(-9,1))
    vals = []
    for ex in exposures:
        cam.set(cv2.CAP_PROP_EXPOSURE, ex)
        _, frame = cam.read()
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
        vals.append(np.mean(frame[:,:,-1]))
        cv2.waitKey(0)

    # find vals closest to target_brightness
    dist = np.array([abs(x-target_brightness) for x in vals])
    print(dist)
    chosen_exposure = exposures[np.argmin(dist)]
    cam.set(cv2.CAP_PROP_EXPOSURE, chosen_exposure)
    print(f"exposure set to {chosen_exposure}")



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
        return (int(x+(1-p)*w), int(y+(1-p)*h))

def get_fist_point_highest(mask, stat):
    x,y,w,h,a = stat_to_normal(stat)
    top_row = mask[y+5,x:x+w]
    one_indices = np.array([i for i in range(len(top_row)) if top_row[i] == 1])
    x_top = np.mean(one_indices).astype(np.int32) + x
    return (x_top,y)



def get_hsv_mask(frame_hsv, low, high):
    # print(frame.shape)
    h_mask1 = np.greater(frame_hsv[:,:,0], low[0])
    h_mask2 = np.less(frame_hsv[:,:,0], high[0])
    h_mask = np.logical_and(h_mask1, h_mask2)

    s_mask1 = np.greater(frame_hsv[:,:,1], low[1])
    s_mask2 = np.less(frame_hsv[:,:,1], high[1])
    s_mask = np.logical_and(s_mask1, s_mask2)

    return np.logical_and(h_mask, s_mask).astype(np.uint8)

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
        x_center = x + w//2
        y_center = y + h//2
        neck_width = int(0.6*w)
        neck_height = int(h)
        # To draw a rectangle in a face
        # cv2.rectangle(pure, (x, int(y-0.15*h)), (x + w, y + int(h * 1.4)), (255, 255, 255))
        cv2.ellipse(pure, (x_center, y_center), (w//2, int(h*1.2)//2), 0, 0, 360, color=(255, 255, 255))
        cv2.rectangle(pure, (x_center-neck_width//2, y_center), (x_center + neck_width//2, y_center + neck_height), (255, 255, 255))
        cv2.ellipse(mask, (x_center, y_center), (w // 2, int(h * 1.2) // 2), 0, 0, 360, color=(255, 255, 255), thickness=-1)
        cv2.rectangle(mask, (x_center - neck_width // 2, y_center),(x_center + neck_width // 2, y_center + neck_height), (255, 255, 255), -1)
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
        # print(faces)
        # print(current_face)
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

def distance(point1, point2):
    return math.sqrt((point1[0] - point2[0])**2 + (point1[1] - point2[1])**2)

def get_closest_point(this_point, other_points):
    ret = other_points[0]
    for p in other_points:
        if distance(p, this_point) < distance(ret, this_point):
            ret = p

def combine_hsv_face_masks(hsv_mask, face_mask):
    mask = np.logical_or(hsv_mask, np.logical_and(hsv_mask, face_mask).astype(np.uint8)).astype(np.uint8)
    return mask

# vs = VideoStream(src=0).start()
vs = cv2.VideoCapture(0)
if not vs.isOpened():
    vs.open(-1)

if not vs.isOpened():
    print("Tried to open the camera but couldn't")
# reset_cam(vs)
adjust_exposure(vs, 50)


UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

while True:

    # vs.set(10, 120)
    # print(HSV_OFFSET)
    # if hsv_mean_right is not None:
    #     print(hsv_mean_right - HSV_OFFSET)
    ret, frame = vs.read()
    frame = imutils.resize(frame, width=WIDTH)
    frame = cv2.flip(frame, 1)
    frame = remove_black_bars(frame)
    # print("frame first:", frame.shape)
    grey_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    hsv_frame = frame
    pure = frame.copy()
    # hsv_frame = cv2.cvtColor(frame, cv2.COLOR_RGB2HSV)


    H, W = frame.shape[:2]
    w, h = W // 16, W // 16
    x1, y1 = 2 * W // 6, H // 2
    x2, y2 = 4 * W // 6, H // 2

    if is_first:
        is_first = False
        first_frame = frame
        first_grey_frame = cv2.cvtColor(first_frame, cv2.COLOR_BGR2GRAY)
        last_fistpoints = [(x1, y1),(x2, y2)]

    if left_sample is None and not sampling_color:
        cv2.putText(frame, f"take hand color sample: press {keys.SAMPLE_HAND.upper()}", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
    elif background is None and not sampling_color:
        cv2.putText(frame, f"take background sample: get out of camera then press {keys.SAMPLE_BACKGROUND.upper()}", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 255), 2)

    # getting hands' color
    if sampling_color:
        # count down from 5
        sec = sampling_time - (time.time() - sampling_start)
        cv2.putText(frame, str(math.ceil(sec)), (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
        cv2.putText(frame, "make a fist, put each square inside a fist", (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
        if sec < 0:
            sampling_color = False
            started = True
            initBB_left = (x1, y1, w, h)
            initBB_right = (x2, y2, w, h)
            left_sample = frame[y1:y1+h, x1:x1+w]
            right_sample = frame[y2:y2+h, x2:x2+w]
            cv2.imwrite("left.png", left_sample)
            cv2.imwrite("right.png", right_sample)
            hsv_mean_left = get_hsv_mean(left_sample)
            hsv_mean_right = get_hsv_mean(right_sample)
            hsv_low_left = hsv_mean_left - HSV_OFFSET
            hsv_high_left = hsv_mean_left + HSV_OFFSET

            hsv_low_right = hsv_mean_right - HSV_OFFSET
            hsv_high_right = hsv_mean_right + HSV_OFFSET

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
        frame_hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV).astype(np.int32)
        frame = frame.astype(np.int32)
        # mask = backSub.apply(frame) // 224


        fg_mask = get_fore_ground_mask(frame, background_int32, BACKGROUND_OFFSET)
        # fg_mask = get_foreground_mask_hsv(hsv_frame, background_hsv, BACKGROUND_OFFSET)

        # hsv_mask_left = get_hsv_mask(frame, hsv_low_left, hsv_high_left)
        hsv_mask_left = get_hsv_mask(frame_hsv, hsv_mean_left - HSV_OFFSET, hsv_mean_left + HSV_OFFSET)
        hsv_mask_right = get_hsv_mask(frame_hsv, hsv_mean_right - HSV_OFFSET, hsv_mean_right + HSV_OFFSET)
        hsv_mask = np.logical_or(hsv_mask_left, hsv_mask_right).astype(np.uint8)

        face_mask = get_face_mask(faces, frame)

        mask = fg_mask * hsv_mask * face_mask
        # mask = fg_mask
        # mask = hsv_mask
        mask = cv2.erode(mask, erode_kernel, iterations=1)
        # mask = remove_small_regions(mask, min_area=5)
        mask_trail.append(mask)
        if len(mask_trail) > MAX_TRAIL_LEN:
            mask_trail.popleft()
        mask = remove_flickering(mask_trail)


        mask = cv2.dilate(mask, dilate_kernel, iterations=1)
        mask_img = (mask * 224).astype(np.uint8)

        connectivity = 4
        num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(mask, connectivity, cv2.CV_32S)

        stats = list(stats[1:])
        stats = sorted(stats, key= lambda x: -x[cv2.CC_STAT_AREA])
        fist_points = []
        for stat in stats:
            if stat[cv2.CC_STAT_AREA] > 1500:
                fist_points.append(get_fist_point_highest(mask, stat))
            if len(fist_points) >= 2:
                break

        if len(fist_points) == 0:
            fist_points = last_fistpoints
        elif len(fist_points) == 1:
            height = fist_points[0][1]
            fist_points = [(W//2 - SMALL_WIDTH, height + 50),(W//2 + SMALL_WIDTH, height + 50)]

        # smaller x means left
        fist_points = sorted(fist_points)
        normalize = []
        for a, b in fist_points:
            a = (a - W // 2) / W
            b = - (b - H//2) / H
            normalize.append(f"{a} {b}")
        sock.sendto(",".join(normalize).encode(), (UDP_IP, UDP_PORT))

        for p in fist_points:
            cv2.circle(mask_img, p, 5, (255,0,0), -1)
            cv2.circle(pure, p, 5, (0, 0, 255), -1)

        # cnts, hierarchy = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        # cnts = sorted(cnts, key=cv2.contourArea)

        frame = frame * np.expand_dims(mask, axis=-1)
        frame = frame.astype(np.uint8)



        fps.update()
        fps.stop()

        frame = mask_img

        cv2.putText(frame, "FPS: %.2f"%(fps.fps()), (10, H-20), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)

    cv2.imshow("Frame", frame)
    cv2.imshow("Pure", pure)
    # cv2.imshow()
    key = cv2.waitKey(1) & 0xFF

    if key == ord(keys.QUIT):
        break
    elif key == ord(keys.SAMPLE_BACKGROUND): # take background picture
        background = frame
        background_int32 = background.astype(np.int32)
    elif key == ord(keys.SAMPLE_HAND): # get hands' color
        # give the user 5 secs to put their hands in the box
        sampling_color = True
        sampling_start = time.time()
    elif key == ord(keys.HUE):
        if not reverse: HSV_OFFSET[0] += 1
        else: HSV_OFFSET[0] -= 1
    elif key == ord(keys.SAT):
        if not reverse: HSV_OFFSET[1] += 1
        else: HSV_OFFSET[1] -= 1
    elif key == ord(keys.VAL):
        if not reverse: HSV_OFFSET[2] += 1
        else: HSV_OFFSET[2] -= 1
    elif key == ord(keys.BACKGROUND_OFFSET):
        if not reverse: BACKGROUND_OFFSET += 1
        else: BACKGROUND_OFFSET -= 1
    elif key == ord(keys.REVERSE):
        reverse = not reverse

vs.release()

