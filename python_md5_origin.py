#
#                    MD5-Zip-Origin
#
# Calculate MD5-Origin of files in folder (Recursive) or zip
# *** (lowercase numbers --> _ --> letters) with / ***
#
#     Т.к. в структуре zip файла присутствуют 
#     last modification time и last modification date,
#     то для одних и тех же файлов в архиве будут разные MD5.
#     По этому, высчитываем MD5 от исходных файлов.
#
# ZIP FORMAT https://en.wikipedia.org/wiki/ZIP_(file_format)#Data_descriptor
#

import os
import hashlib
import zipfile

#
# Расчет MD5 файла
#
def CalculateMD5File(fname: str) -> str:
    # init
    buffz = 16 * 1024 * 1024 # 16 MB
    hash_md5 = hashlib.md5()
    # process
    with open(fname, 'rb') as f:
        for buffr in iter(lambda: f.read(buffz), b''):
            hash_md5.update(buffr)
    return hash_md5.hexdigest().upper()

#
# Расчет MD5 директории (все файлы внутри с именами lowercase)
#
def CalculateMD5Folder(fpath: str) -> str:
    # init
    buffz = 16 * 1024 * 1024 # 16 MB
    hash_md5 = hashlib.md5()
    # get files
    fileList = []
    for wR, wD, wF in os.walk(fpath):
        for f in wF:
            fileList.append(os.path.join(wR, f))
    # важно для последовательного правильного подсчета (lowercase numbers-->_-->letters)
    # c#: fileList.Sort((Comparison<string>)((string s1, string s2) => { return string.CompareOrdinal(s1.ToLower(), s2.ToLower()); }));
    fileList.sort(key = lambda f: ([str,int].index(type(f)), f.lower()))
    # process
    for fileCurr in fileList:
        relativePath = str(fileCurr)[len(fpath)+1:].lower().replace("\\","/")
        if relativePath.endswith('/'): # not a file
            continue;
        # print(relativePath) ### for test sorting
        hash_md5.update(relativePath.encode('utf-8'))
        with open(fileCurr, 'rb') as f:
            for buffr in iter(lambda: f.read(buffz), b''):
                hash_md5.update(buffr)
    return hash_md5.hexdigest().upper()

#
# Расчет MD5 zip архива (все файлы внутри с именами lowercase)
#
def CalculateMD5Zip(fname: str) -> str:
    # init    
    buffz = 16 * 1024 * 1024 # 16 MB
    hash_md5 = hashlib.md5()
    # read zip
    with zipfile.ZipFile(fname, "r") as archive:
        # get files            
        fileList = list(archive.namelist())
        # важно для последовательного правильного подсчета (lowercase numbers-->_-->letters)
        # c#: fileList.Sort((Comparison<string>)((string s1, string s2) => { return string.CompareOrdinal(s1.ToLower(), s2.ToLower()); }));
        fileList.sort(key = lambda f: ([str,int].index(type(f)), f.lower()))
        # process
        for fileCurr in fileList:                
            relativePath = fileCurr.lower().replace("\\","/")
            if relativePath.endswith('/'): # not a file
                continue  
            # print(relativePath) ### for test sorting
            hash_md5.update(relativePath.encode('utf-8'))
            with archive.open(fileCurr, mode='r') as f:
                for buffr in iter(lambda: f.read(buffz), b''):
                    hash_md5.update(buffr)
    return hash_md5.hexdigest().upper()

def Test():
    myPath = r'C:\Downloads\Test'
    myFile = r'C:\Downloads\Test.zip'
    print("MD5-Path :", CalculateMD5Folder(myPath), "~" + myPath[-12:])
    print("MD5-Zip  :", CalculateMD5Zip(myFile), "~" + myFile[-12:])
    print("MD5-File :", CalculateMD5File(myFile), "~" + myFile[-12:])

Test()
