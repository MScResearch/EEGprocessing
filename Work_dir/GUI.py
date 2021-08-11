import os
import time
import threading
def main():
    os.system('python Work_dir\ReceiveData.py')
def monitor():
    os.system('python Work_dir\dataHandler_monitor.py')
def plot():
    os.system('python Work_dir\plot.py')

if __name__ == '__main__':
    x = threading.Thread(target=main)
    y = threading.Thread(target=monitor,daemon=True)
    z = threading.Thread(target=plot)
    x.start()
    y.start()
    time.sleep(5)
    z.start()
    
    

