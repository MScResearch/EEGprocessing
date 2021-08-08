
def clean(fileName,i):
     #----parse data file and remove unwanted characters----
    with open(fileName, 'r') as infile, open('Data/output_streams/output'+str(i)+'.csv', 'w') as outfile:
        data = infile.read()
        data = data.replace("[", "")
        data = data.replace("]", ",")
        #create array here to speed the data collection.
        #full_Data = np.append(full_Data, np.array([[1,2,3]]), axis=0)
        outfile.write(data)
    
if __name__ == '__main__':
    k=0
    fileName="Data/data_streams/dataStreamA"+str(k)+".csv"
    clean(fileName,i)              
    k=i