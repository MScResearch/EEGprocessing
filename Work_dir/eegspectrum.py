import matplotlib.pyplot as plt
from matplotlib import style
import seaborn as sns
import numpy as np
from scipy.integrate import simps
from scipy import signal
import sys
import matplotlib.animation as animation
import time
import ReceiveData


def powerAnalizerAbsolute(fileName,lowV,highV,band):
    data = np.loadtxt(fileName)
    sns.set(font_scale=1.2)
    # Define sampling frequency and time vector
    sf = 500.
    time = np.arange(data.size) / sf
    print(time)
    
    # Plot the signal
    """ fig, ax = plt.subplots(1, 1, figsize=(12, 4))
    plt.plot(time, data, lw=1.5, color='k')
    plt.xlabel('Time (seconds)')
    plt.ylabel('Voltage')
    plt.xlim([time.min(), time.max()])
    plt.title('N3 sleep EEG data (F3)')
    sns.despine() """
    #plt.show()
    #------next bin process------
    win = 4 * sf
    freqs, psd = signal.welch(data, sf, nperseg=win)
    #----------- Define band lower and upper limits---------------
    low, high = lowV, highV
    # Find intersecting values in frequency vector
    idx_delta = np.logical_and(freqs >= low, freqs <= high)
    # Frequency resolution
    freq_res = freqs[1] - freqs[0]  # = 1 / 4 = 0.25
    # Compute the absolute power by approximating the area under the curve
    delta_power = simps(psd[idx_delta], dx=freq_res)
    print('Absolute',band,' power: %.3f uV^2' % delta_power)
    total_power = simps(psd, dx=freq_res)
    return delta_power
    # Plot the power spectrum
    """sns.set(font_scale=1.2, style='white')
    plt.figure(figsize=(8, 4))
    plt.plot(freqs, psd, color='k', lw=2)
    plt.xlabel('Frequency (Hz)')
    plt.ylabel('Power spectral density (V^2 / Hz)')
    plt.ylim([0, psd.max() * 1.1])
    plt.title("Welch's periodogram")
    plt.xlim([0, freqs.max()])
    sns.despine()
    #plt.show() """
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
    freq_res = freqs[1] - freqs[0]  # = 1 / 4 = 0.25
    # Compute the absolute power by approximating the area under the curve
    delta_power = simps(psd[idx_delta], dx=freq_res)
    total_power = simps(psd, dx=freq_res)
    delta_rel_power = delta_power / total_power
    #print('Relative',band,' power: %.3f' % delta_rel_power)

    
    return delta_rel_power
def computeAverages(iter):
        z=iter 
        fileName="output_streams\output"+str(z)+".csv"
        sys.stdout = open("computedAverages\computed"+".csv", "a")                      
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
        #i=i+1
                
    
if __name__ == '__main__':
    
    start_Time=time.time()
    amountFiles=310/10
    for i in range (31):
        computeAverages(i)
    end= time.time()
            
                
            #print(i)            
    
 
    #print ("time for q compute is : ",end-start)
