import time
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler
import os
import dataHandller_linear





def on_created(event):
    os.system('python Work_dir\dataHandller_linear.py')
    


def on_deleted(event):
    print(f"what the f**k! Someone deleted {event.src_path}!")

def on_modified(event):
    os.system('python Work_dir\dataHandller_linear.py')    
    

def on_moved(event):
    print(f"ok ok ok, someone moved {event.src_path} to {event.dest_path}")

    
if __name__ == '__main__':
    
    patterns = ["*"]
    ignore_patterns = None
    ignore_directories = False
    case_sensitive = True
    my_event_handler = PatternMatchingEventHandler(patterns, ignore_patterns, ignore_directories, case_sensitive)
    my_event_handler.on_created = on_created
    my_event_handler.on_deleted = on_deleted
    my_event_handler.on_modified = on_modified
    my_event_handler.on_moved = on_moved
    path = "Data/data_streams/"
    go_recursively = True
    my_observer = Observer()
    my_observer.schedule(my_event_handler, path, recursive=go_recursively)
   
    my_observer.start()
    try:
        while True:
            k=0
            time.sleep(1)
    except KeyboardInterrupt:
        my_observer.stop()
        my_observer.join()




   
    #fileName="Data/data_streams/dataStreamA"+str(k)+".csv"
    #clean(fileName,k)              
    #k=i