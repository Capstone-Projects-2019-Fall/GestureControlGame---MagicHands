# import cv2
# import numpy as np
#
# src = cv2.imread("paint.png", 0)
# bin = (src > 150).astype(np.uint8)
# connectivity = 0 # or whatever you prefer
#
#
# num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(bin, connectivity, cv2.CV_32S)
# bin = (bin * 100).astype(np.uint8)
#
# for x,y in centroids:
#     cv2.circle(bin, (int(x),int(y)), 10, (255,255,255))
#
# cv2.imshow("Frame", bin)
# key = cv2.waitKey() & 0xFF

# # Python program to demonstrate erosion and
# # dilation of images.
# import cv2
# import numpy as np
#
# # Reading the input image
# img = cv2.imread('paint.png', 0)
#
# # Taking a matrix of size 5 as the kernel
# ker_size = 10
# kernel = np.ones((ker_size, ker_size), np.uint8)
#
# # The first parameter is the original image,
# # kernel is the matrix with which image is
# # convolved and third parameter is the number
# # of iterations, which will determine how much
# # you want to erode/dilate a given image.
# img_erosion = cv2.erode(img, kernel, iterations=1)
# img_dilation = cv2.dilate(img, kernel, iterations=1)
#
# cv2.imshow('Input', img)
# cv2.imshow('Erosion', img_erosion)
# cv2.imshow('Dilation', img_dilation)
#
# cv2.waitKey(0)

import numpy as np

a = np.array([1,1,0,0], np.uint8)
b = np.array([1,0,1,0], np.uint8)

c = np.logical_or(a,b).astype(np.uint8)
print(c)