import sys
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd
import time
from numpy import savetxt




if __name__ == '__main__':   
    
    
    #datas= datas[~np.isnan(datas)]
    #savetxt('data.csv', datas, delimiter=',')
    
    for i in range(1000):
        datas = np.genfromtxt("Package\Data\computedAverages\computed.csv")       
        fig1=plt.figure()
        plt.plot(datas[:,0],label="Delta")
        plt.plot(datas[:,2],label="Theta")
        plt.plot(datas[:,4],label="Alpha")
        plt.plot(datas[:,6],label="Beta")
        plt.plot(datas[:,8],label="Gama")
        plt.show(block=False)
        plt.pause(10)
        plt.close()
        print(i)
    """ fig2=plt.figure()
    plt.plot(datas[:,2],label="Beta")
    plt.show(block=False)
    plt.pause(3)
    plt.close() """
        
        
    