

from pylsl import StreamInlet, resolve_stream
import sys
import time
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import numpy as np
from scipy.integrate import simps
from scipy import signal
import eegspectrum



def main(epochTime,fileNumber):
    i=0
    # first resolve an EEG stream on the lab network    
    streams = resolve_stream('type', 'EEG')
    #start file system for recording.    
    inlet = StreamInlet(streams[0]) 
    sys.stdout = open("Data/data_streams/dataStreamA"+str(fileNumber)+".csv", "w")      
    start_time= time.time() 
    #------read from stream with time pased as argument-------        
    while i<epochTime:
        # get a new sample (you can also omit the timestamp part if you're not
        # interested in it)
        offset = inlet.time_correction()
        #print('Offset: ' + str(offset))
        sample, timestamp = inlet.pull_sample()
        measureTime=time.time()
        print(sample,measureTime-start_time)
        #print(sample)
        #print(timestamp-offset)
        i=measureTime-start_time            
    #sys.stdout.close()

def read(fileName):
    #----make sure data is consistent-----
    data=pd.read_csv(fileName, header=None)
    #print (data)

def clean(fileName,i):
    #----parse data file and remove unwanted characters----
    with open(fileName, 'r') as infile, open('Data/output_streams/output'+str(i)+'.csv', 'w') as outfile:
        data = infile.read()
        data = data.replace("[", "")
        data = data.replace("]", ",")
        #create array here to speed the data collection.
        #full_Data = np.append(full_Data, np.array([[1,2,3]]), axis=0)
        outfile.write(data)
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
def computeAverages(fileName,z):        
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
        #i=i+1
                
if __name__ == '__main__':
    studyTime=310
    epochTime=2
    fileLenght=studyTime/epochTime
    #created the data array to work online in a faster method
    #full_Data=np.empty(0,5)    
    for i in range(int(fileLenght)):
        fileName="Data/data_streams/dataStreamA"+str(i)+".csv"
        main(epochTime,i)
        clean(fileName,i)        
        output="Data/output_streams/output"+str(i)+".csv"
        computeAverages(output,i)
        # dont plot on the same script, ploting requires different tread, otherwise samples are droped with pause.
        
        

       

