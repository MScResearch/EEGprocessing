

from pylsl import StreamInlet, resolve_stream
import sys
import time
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import numpy as np
from scipy.integrate import simps
from scipy import signal
import threading
import os



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

        #i=i+1
                
if __name__ == '__main__':
    studyTime=310
    epochTime=2
    fileLenght=studyTime/epochTime
    k=0
    #created the data array to work online in a faster method
    #full_Data=np.empty(0,5)    
    for i in range(int(fileLenght)):               
        main(epochTime,i)               
        
        # dont plot on the same script, ploting requires different tread, otherwise samples are droped with pause.
        
        

       

