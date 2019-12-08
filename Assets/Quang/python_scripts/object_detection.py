import cv2
import matplotlib.pyplot as plt
import cvlib as cv
from cvlib.object_detection import draw_bbox
# im = cv2.imread('hands.jpg')
# bbox, label, conf = cv.detect_common_objects(im)
# output_image = draw_bbox(im, bbox, label, conf)
# plt.imshow(output_image)
# plt.show()

vs = cv2.VideoCapture(0)
while True:
    ret, frame = vs.read()
    frame = cv2.flip(frame, 1)
    bbox, label, conf = cv.detect_common_objects(frame)
    output_image = draw_bbox(frame, bbox, label, conf)
    cv2.imshow("frame", frame)
    key = cv2.waitKey(1) & 0xFF
    if key == ord("q"):
        break