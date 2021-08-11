import os
import sys
import time
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import numpy as np
from scipy.integrate import simps
from scipy import signal
def powerAnalizerRelative(fileName,lowV,highV,column):
    fullData=np.genfromtxt(fileName,delimiter=',')
    data=fullData[:,column]
    #data = np.loadtxt(fileName)
    sns.set(font_scale=1.2)
    # Define sampling frequency and time vector
    sf = 100.
    time = np.arange(data.size) / sf 
    #------next bin process------
    win = 4 * sf
    freqs, psd = signal.welch(data, sf, nperseg=win)
    #----------- Define band lower and upper limits---------------
    low, high = lowV, highV
    # Find intersecting values in frequency vector
    idx_delta = np.logical_and(freqs >= low, freqs <= high)
    # Frequency resolution
    freq_res = freqs[1] - freqs[10]  # = 1 / 4 = 0.25
    # Compute the absolute power by approximating the area under the curve
    delta_power = simps(psd[idx_delta], dx=freq_res)
    total_power = simps(psd, dx=freq_res)
    delta_rel_power = delta_power / total_power
    #print('Relative',band,' power: %.3f' % delta_rel_power) 
    return delta_rel_power
def computeAverages(fileName):        
        sys.stdout = open("Data/computedAverages/computed"+".csv", "a")                      
        #Delta wave – (0.5 – 3 Hz)
        #Theta wave – (4 – 7 Hz)
        #Alpha wave – (8 – 12 Hz)
        #Mu wave – (7.5 – 12.5 Hz)
        #SMR wave – (12.5 – 15.5 Hz)
        #Beta wave – (15 – 30 Hz)
        #Gamma wave – (>30 Hz)
        for k in range (6):
            D=powerAnalizerRelative(fileName,0.5,3,k)
            T=powerAnalizerRelative(fileName,4,7,k)
            A=powerAnalizerRelative(fileName,8,12,k)
            B=powerAnalizerRelative(fileName,15,30,k)
            G=powerAnalizerRelative(fileName,30,45,k)
            print(D,",",T,",",A,",",B,",",G,",")
            #sys.stdout = close("computedAverages\computed"+str(z)+".csv")

APP_FOLDER = "Data\data_streams"
totalFiles = 0
fileName="Data/data_streams/dataStreamA"+str(totalFiles)+".csv"
computeName='Data/output_streams/output'+str(totalFiles)+'.csv'

for base, dirs, files in os.walk(APP_FOLDER):
    for Files in files:
        with open(fileName, 'r') as infile, open('Data/output_streams/output'+str(totalFiles)+'.csv', 'w') as outfile:
            data = infile.read()
            data = data.replace("[", "")
            data = data.replace("]", ",")
            #create array here to speed the data collection.
            #full_Data = np.append(full_Data, np.array([[1,2,3]]), axis=0)
            outfile.write(data)
        computeAverages(computeName)    
        totalFiles += 1

