import numpy as np
import pandas as pd

class LinearRegression():
    def __init__(self, X, y, alpha=0.03, n_iter=1500):

        self.alpha = alpha
        self.n_iter = n_iter
        self.n_samples = len(y)
        self.n_features = np.size(X, 1)
        self.X = np.hstack((np.ones(
            (self.n_samples, 1)), (X - np.mean(X, 0)) / np.std(X, 0)))
        self.y = y[:, np.newaxis]
        self.params = np.zeros((self.n_features + 1, 1))
        self.coef_ = None
        self.intercept_ = None

    def fit(self):

        for i in range(self.n_iter):
            self.params = self.params - (self.alpha/self.n_samples) * \
            self.X.T @ (self.X @ self.params - self.y)

        self.intercept_ = self.params[0]
        self.coef_ = self.params[1:]

        return self

    def score(self, X=None, y=None):

        if X is None:
            X = self.X
        else:
            n_samples = np.size(X, 0)
            X = np.hstack((np.ones(
                (n_samples, 1)), (X - np.mean(X, 0)) / np.std(X, 0)))

        if y is None:
            y = self.y
        else:
            y = y[:, np.newaxis]

        y_pred = X @ self.params
        score = 1 - (((y - y_pred)**2).sum() / ((y - y.mean())**2).sum())

        return score

    def predict(self, X):
        n_samples = np.size(X, 0)
        y = np.hstack((np.ones((n_samples, 1)), (X-np.mean(X, 0)) / np.std(X, 0))) @ self.params
        return y

    def get_params(self):
        return self.params


data = pd.read_csv("up.csv", header=None).values
# lg = LinearRegression(data[:,:-1], data[:,-1], n_iter=500)
# lg.fit()
# score = lg.score(data[:,:-1], data[:,-1])
# print(score)
# print(lg.predict(data[:3,:-1]))

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


X = data[:,:-1]
y = data[:,-1]
print(y.shape)
model = MyLinearRegression(X, y)
print(model.loss())
model.fit()
print(model.loss())
# print(model.predict(X))
print(X[0])
print(model.predict(X[0]))