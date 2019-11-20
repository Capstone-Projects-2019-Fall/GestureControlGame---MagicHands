import numpy as np

x = np.random.normal(size=[1000, 2])
x[:,1] = x[:,1] * 0.1

cov = np.cov(x.T)

e_val, e_vec = np.linalg.eig(cov)

# e_val[i] corresponds to e_vec[:,i]
a = x.dot(e_vec)

k = np.array([[1,1], [-1, -1]])
k = k.dot(e_vec)


print()

class PCA:
    def __init__(self):
        pass

    def fit(self, x):
        cov = np.cov(x.T)
        e_val, e_vec = np.linalg.eig(cov)
        sorted_idx = np.argsort(-np.abs(e_val))
        self.sorted_e_vec = e_vec[:, sorted_idx]

    def transform(self, x, num_components):
        return x.dot(self.sorted_e_vec[:,:num_components])


pca = PCA()
pca.fit(x)
b = pca.transform(x, 1)

print()