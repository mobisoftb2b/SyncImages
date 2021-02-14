cd C:\Mobisoft_B2B\images\
DEL /F/Q/S -rf Pictures
DEL /F/Q/S -rf Pictures_large
%rmdir /F/Q/S -rf Pictures_large
xcopy /s D:\mtnout_android\mobisoft\Media\Pictures C:\Mobisoft_B2B\images
DEL /F/Q/S -rf Pictures\Old
DEL /F/Q/S -rf Pictures_large\Old
cd C:\Mobisoft_B2B\images\PIctures
rmdir /Q/S -rf Old
cd C:\Mobisoft_B2B\images\Pictures_large
rmdir /Q/S -rf Old
cd C:\Mobisoft_B2B\images\

