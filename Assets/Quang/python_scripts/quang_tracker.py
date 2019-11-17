from imutils.video import FPS
import imutils
import cv2
import socket
import time
import math
import numpy as np
import collections
import argparse
from os.path import join


ap = argparse.ArgumentParser()
ap.add_argument("-F", "--face", type=str, default="haarcascade_frontalface_default.xml", help="path to face cascade file")
ap.add_argument("-H", "--hue", type=int, default=7, help="hue offset")
ap.add_argument("-S", "--saturation", type=int, default=36, help="saturation offset")
ap.add_argument("-V", "--value", type=int, default=40, help="value offset")
ap.add_argument("-B", "--background", type=int, default=10, help="background offset")
ap.add_argument("-C", "--custom", type=int, default=0, help="whether to use custom motion control or not")
ap.add_argument("-O", "--output", type=str, default="", help="the directory to output data files")
ap.add_argument("-L", "--load", type=int, default=1, help="load previously saved custom motion control")

args = vars(ap.parse_args())

class MyLinearRegression:
    def __init__(self, X, y):
        if len(y.shape) < 2:
            y = np.expand_dims(y, 1)
        num_features = X.shape[1]
        num_outputs = y.shape[1]
        self.X = X
        self.y = y
        self.W = np.random.normal(size=[num_features, num_outputs])
        self.b = np.random.normal(size=[1, num_outputs])

    def predict(self, X):
        if len(X.shape) < 2:
            X = np.expand_dims(X, 0)
        return np.matmul(X, self.W) + self.b

    def fit(self, lr=0.02, n_iter=2000):
        for i in range(n_iter):
            dy = -2*(self.y - self.predict(self.X))
            dw = np.mean(np.matmul(np.expand_dims(self.X,-1), np.expand_dims(dy, 1)), axis=0)
            db = np.mean(dy, axis=0, keepdims=True)

            self.W -= dw * lr
            self.b -= db * lr
        return self

    def loss(self):
        return np.sqrt(np.mean((self.y - self.predict(self.X))**2))

class PCA:
    def __init__(self):
        pass

    def fit(self, x):
        cov = np.cov(x.T)
        e_val, e_vec = np.linalg.eig(cov)
        sorted_idx = np.argsort(-np.abs(e_val))
        self.sorted_e_vec = e_vec[:, sorted_idx]

    def transform(self, x, num_components=1):
        return x.dot(self.sorted_e_vec[:,:num_components])


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
MAX_TRAIL_LEN = 1
last_fistpoints = []
SMALL_WIDTH = 20
reverse = False
custom_control = args["custom"]
is_in_data_process = False
data_names = {"right":[], "up":[], "roll":[], "speed":[]}
data_names_list = list(data_names.keys())
current_data_index = 0
is_preparing_for_data_collection = False
is_in_data_collection = False
preparing_time = 3
collecting_time = 2
preparing_start = None
collecting_start = None
levels = list(range(-2,3))
current_level_index = 0
done_with_data = False
load = args["load"] == 1
finished_training = False
models = {}

tutorial_sentences = {"right": ["turning left a lot", "turning left a little", "no turning left or right", "turning right a little", "turning right a lot"],
                      "up":    ["turning down a lot", "turning down a little", "no turning up or down", "turning up a little", "turning up a lot"],
                      "roll":  ["rolling left fast", "rolling left slow", "no rolling", "rolling right slow", "rolling right fast"],
                      "speed": ["speed level 1/5", "speed level 2/5",  "speed level 3/5", "speed level 4/5", "speed level 5/5"]}



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
        self.NEXT = "n"
        self.EXPOSURE = "e"

def get_trained_model(data):
    # data shape (?, 3)
    model = MyLinearRegression(data[:,:-1], data[:,-1])
    model.fit()
    return model


class PCAModel:
    def __init__(self, pca, model):
        self.pca = pca
        self.model = model

    def predict(self, data):
        if len(data.shape) < 2:
            data = np.expand_dims(data, 0)
        sub_data = self.pca.transform(data)
        return self.model.predict(sub_data)

def get_trained_pca_model(data):
    # data shape (?, 3)
    pca = PCA()
    pca.fit(data[:, :-1])
    sub_x = pca.transform(data[:,:-1])
    model = MyLinearRegression(sub_x, data[:, -1])
    model.fit()
    pcaModel = PCAModel(pca, model)
    return pcaModel

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
    return chosen_exposure


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
        neck_width = int(0.7*w)
        neck_height = int(1.6*h)
        # To draw a rectangle in a face
        # cv2.rectangle(pure, (x, int(y-0.15*h)), (x + w, y + int(h * 1.4)), (255, 255, 255))
        cv2.ellipse(pure, (x_center, y_center-int(h/10)), (w//2, int(h*1.4)//2), 0, 0, 360, color=(255, 255, 255))
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

def my_read_csv(path):
    ret = []
    with open(path) as f:
        for line in f:
            line = line.strip()
            ret.append(line.split(","))
    return np.array(ret, np.float32)

def resize(frame, width):
    old_h, old_w = frame.shape[:2]
    scale = width / old_w
    height = int(scale * old_h)
    return cv2.resize(frame, (width, height))


# vs = VideoStream(src=0).start()
vs = cv2.VideoCapture(0)
if not vs.isOpened():
    vs.open(-1)

if not vs.isOpened():
    print("Tried to open the camera but couldn't")

# reset_cam(vs)
chosen_exposure = adjust_exposure(vs, 50)


UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

while True:
    vs.set(cv2.CAP_PROP_EXPOSURE, chosen_exposure)
    # vs.set(10, 120)
    # print(HSV_OFFSET)
    # if hsv_mean_right is not None:
    #     print(hsv_mean_right - HSV_OFFSET)
    ret, frame = vs.read()
    # frame = imutils.resize(frame, width=WIDTH)
    # frame = cv2.resize(frame, (WIDTH, ))
    frame = resize(frame, WIDTH)
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

    # the main loop
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

        frame = mask_img
        connectivity = 4
        num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(mask, connectivity, cv2.CV_32S)

        stats = list(stats[1:])
        stats = sorted(stats, key= lambda x: -x[cv2.CC_STAT_AREA])
        fist_points = []
        for stat in stats:
            if stat[cv2.CC_STAT_AREA] > 3000:
                fist_points.append(get_fist_point_highest(mask, stat))
            if len(fist_points) >= 2:
                break

        # if len(fist_points) == 0:
        #     fist_points = last_fistpoints
        # elif len(fist_points) == 1:
        #     height = fist_points[0][1]
        #     fist_points = [(W//2 - SMALL_WIDTH, height + 50),(W//2 + SMALL_WIDTH, height + 50)]



        # smaller x means left
        # fist_points = sorted(fist_points) # has at most 2 elements.
        normalized_fist_points = []
        for a, b in fist_points:
            a = (a - W // 2) / W
            b = - (b - H//2) / H
            normalized_fist_points.append([a,b])


        for p in fist_points:
            cv2.circle(mask_img, p, 5, (255,0,0), -1)
            cv2.circle(pure, p, 5, (0, 0, 255), -1)

        cv2.line(frame, (W // 2, 0), (W // 2, H), 50, 2)
        cv2.circle(frame, (W // 4, H // 2), 5, 120, -1)
        cv2.circle(frame, (3 * W // 4, H // 2), 5, 120, -1)

        if not is_in_data_process and not done_with_data:
            if current_data_index < len(data_names):
                cv2.putText(frame, f"next is {data_names_list[current_data_index]}, press {keys.NEXT.upper()}", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, 120, 2)


        if is_in_data_process and not done_with_data:
            # print(is_in_data_process)
            if not is_in_data_collection and not is_preparing_for_data_collection:
                is_preparing_for_data_collection = True
                preparing_start = time.time()
            if is_preparing_for_data_collection:
                sec_preparing = preparing_time - (time.time() - preparing_start)
                cv2.putText(frame, str(math.ceil(sec_preparing)), (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, 120, 2)
                # cv2.putText(frame, f"prepare for {data_names_list[current_data_index]} level {levels[current_level_index]}", (10, 50), cv2.FONT_HERSHEY_SIMPLEX,0.6, 120, 2)
                cv2.putText(frame, f"prepare for {tutorial_sentences[data_names_list[current_data_index]][current_level_index]}", (10, 50), cv2.FONT_HERSHEY_SIMPLEX,0.6, 120, 2)
                if sec_preparing <= 0:
                    is_preparing_for_data_collection = False
                    is_in_data_collection = True
                    collecting_start = time.time()
            if is_in_data_collection:
                sec_collecting = collecting_time - (time.time() - collecting_start)
                cv2.putText(frame, str(math.ceil(sec_collecting)), (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, 120,2)
                cv2.putText(frame,
                            # f"collecting {data_names_list[current_data_index]} level {levels[current_level_index]}",
                            f"collecting {tutorial_sentences[data_names_list[current_data_index]][current_level_index]}",
                            (10, 50), cv2.FONT_HERSHEY_SIMPLEX,
                            0.6, 120, 2)
                if len(normalized_fist_points) > 0:
                    current_data = data_names_list[current_data_index]
                    data_names[current_data].append(f"{','.join([str(_) for _ in normalized_fist_points[0]])},{levels[current_level_index]}\n")
                if sec_collecting <= 0:
                    is_in_data_collection = False
                    current_level_index = (current_level_index + 1) % len(levels)
                    if current_level_index == 0:
                        current_data_index = current_data_index + 1
                        is_in_data_process = False
                    if current_data_index >= len(data_names_list):
                        done_with_data = True
                        for name in data_names:
                            with open(join(args['output'],f"{name}.csv"), "w") as f:
                                f.writelines(data_names[name])
        if done_with_data and not finished_training and custom_control==1:
            for name in data_names:
                model_data = my_read_csv(join(args['output'],f"{name}.csv"))
                models[name] = get_trained_pca_model(model_data)
            finished_training = True

        normalized_fist_points = sorted(normalized_fist_points)

        normalized_left_center = [-0.25, 0]
        normalized_right_center = [0.25, 0]
        if not is_in_data_process and done_with_data:
            if custom_control == 1:
                if len(normalized_fist_points) == 0:
                    normalized_fist_points = [normalized_left_center, normalized_right_center]
                elif len(normalized_fist_points) == 1: # only right hand
                    normalized_fist_points = [normalized_left_center] + normalized_fist_points
                normalized_fist_points = np.array(normalized_fist_points, np.float32)
                up_ = models["up"].predict(normalized_fist_points[1])[0,0]
                right_ = models["right"].predict(normalized_fist_points[1])[0,0]
                roll_ = models["roll"].predict(normalized_fist_points[0])[0,0]
                speed_ = models["speed"].predict(normalized_fist_points[0])[0,0]
                sock.sendto(f"{right_/4} {up_/4} {roll_/4} {speed_/4}".encode(), (UDP_IP, UDP_PORT))
                print(f"right: {right_}, up: {up_}, roll: {roll_}, speed: {speed_}")

            else:
                normalize = []
                for a, b in normalized_fist_points:
                    normalize.append(f"{a} {b}")
                for i in range(len(normalize), 2):
                    normalize.append("x x")

                sock.sendto(",".join(normalize).encode(), (UDP_IP, UDP_PORT))

        # frame = frame * np.expand_dims(mask, axis=-1)
        # frame = frame.astype(np.uint8)



        fps.update()
        fps.stop()

        cv2.putText(frame, "FPS: %.2f"%(fps.fps()), (10, H-20), cv2.FONT_HERSHEY_SIMPLEX, 0.6, 120, 2)


    cv2.imshow("Frame", frame)
    # cv2.imshow("Pure", pure)
    # cv2.imshow()
    key = cv2.waitKey(1) & 0xFF

    if key == ord(keys.QUIT):
        break
    elif key == ord(keys.SAMPLE_BACKGROUND): # take background picture
        background = frame
        background_int32 = background.astype(np.int32)
        if custom_control == 0 or load==True:
            done_with_data = True
    elif key == ord(keys.SAMPLE_HAND): # get hands' color
        # give the user 5 secs to put their hands in the box
        sampling_color = True
        sampling_start = time.time()
    elif key == ord(keys.HUE):
        if not reverse: HSV_OFFSET[0] += 1
        else: HSV_OFFSET[0] -= 1
        print("Hue:", HSV_OFFSET[0])
    elif key == ord(keys.SAT):
        if not reverse: HSV_OFFSET[1] += 1
        else: HSV_OFFSET[1] -= 1
        print("Sat:", HSV_OFFSET[1])
    elif key == ord(keys.VAL):
        if not reverse: HSV_OFFSET[2] += 1
        else: HSV_OFFSET[2] -= 1
        print("Val:", HSV_OFFSET[2])
    elif key == ord(keys.BACKGROUND_OFFSET):
        if not reverse: BACKGROUND_OFFSET += 1
        else: BACKGROUND_OFFSET -= 1
    elif key == ord(keys.REVERSE):
        reverse = not reverse
    elif key == ord(keys.NEXT):
        is_in_data_process = True
    elif key == ord(keys.EXPOSURE):
        if not reverse: chosen_exposure += 1
        else: chosen_exposure -= 1
        print("exposure:",chosen_exposure)
        vs.set(cv2.CAP_PROP_EXPOSURE, chosen_exposure)
vs.release()
