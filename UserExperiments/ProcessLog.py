# open files

pathToOriginalLogs = r"UserLogs\OriginalLogs" + "\\"

fileNameOne = r"Log-20-01-2023_17-38-14" 
fileNameTwo = r"Log-20-01-2023_17-38-11" 

fileExtension = r".txt"

timeCompletingMinutes = 8
timeCompletingSeconds = 41

# -----------------------------------------------------------------------

fileOne = open(pathToOriginalLogs + fileNameOne + fileExtension, "r") 
fileTwo = open(pathToOriginalLogs + fileNameTwo + fileExtension, "r") 

# read files as a single string (that has \n)
dataFileOneString = fileOne.read()
dataFileTwoString = fileTwo.read()

# close files
fileOne.close()
fileTwo.close()

# split each line of the string in a list
dataFileOneList = dataFileOneString.splitlines()
dataFileTwoList = dataFileTwoString.splitlines()

# list of special characters
forcedCommitChar = "*"
requestCommitChar = "%"
requestCommitAcceptChar = "!"
requestCommitDeclineChar = "$"
forcedDeletionChar = "&"
requestDeletionChar = "?"
requestDeletionAcceptChar = "+"
requestDeletionDeclineChar = "="

specialChars = [forcedCommitChar, requestCommitChar, requestCommitAcceptChar, requestCommitDeclineChar, 
                forcedDeletionChar, requestDeletionChar, requestDeletionAcceptChar, requestDeletionDeclineChar]

# -----------------------------------------------------------------------

# clean up first lines by removing all commit/deletion operations until a normal line is reached
for x in dataFileOneList[:]:
    if any(element == x[0] for element in specialChars):
        dataFileOneList.remove(x)
    else:
        break

for x in dataFileTwoList[:]:
    if any(element == x[0] for element in specialChars):
        dataFileTwoList.remove(x)
    else:
        break

# -----------------------------------------------------------------------

# count commits/deletions lines

countSpecialLinesFOne = 0
countSpecialLinesFTwo = 0

specialLinesFOne = []
specialLinesFTwo = []

# for each line in the first file, save 'special' lines because they refer to commit/deletion operations
# (check if first char has at least one of the special char that marks the line)
# moreover, remove them from the original list
for x in dataFileOneList[:]:
    if any(element == x[0] for element in specialChars):
        specialLinesFOne.append(x)
        dataFileOneList.remove(x)

for x in dataFileTwoList[:]:
    if any(element == x[0] for element in specialChars):
        specialLinesFTwo.append(x)
        dataFileTwoList.remove(x)

# -----------------------------------------------------------------------

# Due to the nature of the log program, if a user sends an accept, the line immediately before is  
# logged as a forced operation. To solve this, simply remove the previous line

for x in specialLinesFOne[:]:
    if x[0] == requestCommitAcceptChar or x[0] == requestDeletionAcceptChar:
        if previous[0] == forcedCommitChar or previous[0] == forcedDeletionChar:
            specialLinesFOne.remove(previous)
    
    previous = x

for x in specialLinesFTwo[:]:
    if x[0] == requestCommitAcceptChar or x[0] == requestDeletionAcceptChar:
        if previous[0] == forcedCommitChar or previous[0] == forcedDeletionChar:
            specialLinesFTwo.remove(previous)
    
    previous = x

countSpecialLinesFOne = len(specialLinesFOne)
countSpecialLinesFTwo = len(specialLinesFTwo)

print(countSpecialLinesFOne, countSpecialLinesFTwo)

# -----------------------------------------------------------------------

# using the completion time in the experiment, convert mm:ss in just seconds and truncate the list to that number
# (works because I logged every second)

timeCompletingTotalSeconds = timeCompletingMinutes * 60 + timeCompletingSeconds

dataFileOneList = dataFileOneList[:timeCompletingTotalSeconds]
dataFileTwoList = dataFileTwoList[:timeCompletingTotalSeconds]

# -----------------------------------------------------------------------

# save to new files, using a suffix

pathToCleanedLogs = r"UserLogs\CleanedLogs\Logs" + "\\"
pathToCleanedOperations = r"UserLogs\CleanedLogs\Operations" + "\\"

suffixLogFile = r"-cleanedLog"
suffixOperationFile = r"-operations"

# file 1 log
fileOneLogToSave = open(pathToCleanedLogs + fileNameOne + suffixLogFile + fileExtension, 'w')
for item in dataFileOneList:
	fileOneLogToSave.write(item+"\n")
fileOneLogToSave.close()

# file 2 log
fileTwoLogToSave = open(pathToCleanedLogs + fileNameTwo + suffixLogFile + fileExtension, 'w')
for item in dataFileTwoList:
	fileTwoLogToSave.write(item+"\n")
fileTwoLogToSave.close()

# file 1 operations
fileOneOperToSave = open(pathToCleanedOperations + fileNameOne + suffixOperationFile + fileExtension, 'w')
for item in specialLinesFOne:
	fileOneOperToSave.write(item+"\n")
fileOneOperToSave.close()

# file 2 operations
fileTwoOperToSave = open(pathToCleanedOperations + fileNameTwo + suffixOperationFile + fileExtension, 'w')
for item in specialLinesFTwo:
	fileTwoOperToSave.write(item+"\n")
fileTwoOperToSave.close()

# -----------------------------------------------------------------------

# Prints

print(len(dataFileOneList), len(dataFileTwoList))

print("File one special lines")
for x in specialLinesFOne:
    print(x)

print()

print("File two special lines")
for x in specialLinesFTwo:
    print(x)


# assuming users pushed the wrong button during the task, remove request operations from forced tasks and vice versa
# manually since the tests were not too many
# oppure anche no? boh. proviamo a far combaciare due file di operazioni dei due utenti e poi sort? boh. intanto niente 