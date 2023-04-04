In this folder, logs are split in 2 different files: one containing only user activities and one containing only operations.

A python script opens the files pair by pair
Deletes the first lines if they represent operations, until an activity line is reached. This is because the system also records operations in the tutorial part, so it is necessary to delete these lines. 
Then, the script checks for operation lines by matching the first character of every line to the convention described in this Subsection. If an operation line is found, it is moved to a separate list and deleted from the original file (Operations folder) 
Next, due to the nature of the logging program, if a user sends an accept operation the line immediately before is recorded as a forced operation. To solve this, simply remove the previous line. The original file now contains only activity lines, so it can be trimmed to the same time length as the task: convert minutes:seconds in just seconds and truncate the file to that number.

The script writes the cleaned-up data in two different files: 
the first contains the trimmed activity lines and is called Log-dd-MM-yyy_HH-mm-ss-cleanedLog.txt (UserLog folder)
the second contains the operation lines and is called Log-dd-MM-yyy_HH-mm-ss-operations.txt (Operations folder)