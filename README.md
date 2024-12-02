<p align="center">
  <img src="https://github.com/user-attachments/assets/49669f88-cfdb-4c0f-b0cb-0ab641355fb5" width="256" height="125" />

</p>


## Battery Monitor  
An app that monitoring when the device battery is getting too low and needs to be recharged, or when the device is charging and the charge is high.
When the thresholds are reached then it bores and stresses the user with a notification.



<a href="https://play.google.com/store/apps/details?id=com.toyokenstudio.batterymonitor"><img src="https://github.com/user-attachments/assets/5b1c1cbb-24cf-4de3-9d49-5cd2db008c99" width="170" /></a>  
<a href="https://play.google.com/store/apps/details?id=com.toyokenstudio.batterymonitor">Download from Google Play Store</a> 


The app at the moment is available only for __Android__.  
Once started, the app is running in background and is always active, even if the device is rebooted the app start automatically. 

*Why this app?*  
As known, to preserve the health of modern batteries is better to avoid full charge cycles (0-100%) and overnight charging. 
Furthermore, limiting smartphone's maximum charge to 80-90% is better for the battery's health than topping up to completely full everytime.  
With this app you could set the topper threshold and the lower threshold and the phone will notify you when those thresholds are reached.  
It's not necessary to open the app every time: the service starts automatically when phone boot and is always running in background.  


For technical and theorical details, please refers to [References section](#references).

## Screenshots
<img src="https://github.com/user-attachments/assets/30963ee3-36fb-457c-befd-0d1c65d827f5" width="300" height="650">

<img src="https://github.com/user-attachments/assets/fcc7453a-cdb1-46ca-b4b0-aa1ca0a0e04c" width="300" height="650">   

<img src="https://github.com/user-attachments/assets/3ec4d23f-3716-4eda-9d35-0abda206d766" width="300" height="650">   

<img src="https://github.com/user-attachments/assets/a90a1b9f-171c-47b3-9e44-b49b9559a9fa" width="300" height="650">

<img src="https://github.com/user-attachments/assets/8a6ed42c-0f46-4e66-ba35-0fe06f26f936" width="300" height="650">

<img src="https://github.com/user-attachments/assets/421632ff-a385-4637-9087-2241396ce030" width="300" height="650">


## Features
- MVVM (MVVM Community Toolkit)
- Reusable components
- Localizzation (English and Italian, German coming soon)
- Dark/Light theme support
- SkiaSharp 2D (just for the fun of it)
- Platform specific features
  

## Known issues
- When opening "Settings" page the first time there is a little lag in the opening of the settings page.
- On some devices with custom version of Android sometimes app shut down and doesn't stay active in background. Please refer to [FAQ](#faq) for this problem.


## FAQ
- *This app is always active in background. Will it drain my battery?*  
No, even if the app remains active in the background, it is a very lightweight process, whose load on the battery is practically negligible.

- *When I press "clear all" app button, the service is not running anymore (for **Xiaomi/Redmi phone</b>)***  
If you are an _unfortunate owner_ of a **Xiaomi/Redmi** phone (like me 😓) there are a procedure to do in order to avoid service killing and make the service run in the background.  
You can find more information here: [Don't kill my app](https://dontkillmyapp.com/xiaomi)  

- *Can I change the notification sound?*
Sure. Go to _Settings_ > _Apps_ > _All apps_ > _Battery Monitor_ > _Notifications.  
Tap on "Battery levels changes". In the notification settings you can change the sound tapping on "Sound".  

- *There will be a version for other OSs?*  
No.
(Maybe for Windows if I will have free spare time...)


## Credits
#### Fonts
[PermanentMarker-Regular](https://fonts.google.com/specimen/Permanent+Marker)  


## References
Here you can find some interesting articles about the correct use of the battery:

https://www.geopop.it/come-non-rovinare-la-batteria-dello-smartphone-5-suggerimenti-utili/

https://www.geopop.it/caricare-e-scaricare-completamente-la-batteria-del-cellulare-e-sbagliato-ecco-cosa-fare/

https://www.androidauthority.com/Maximize-battery-life-882395/

https://www.tecnoandroid.it/2024/03/15/come-caricare-il-cellulare-in-modo-corretto-1364482/

https://www.howtogeek.com/896953/phone-battery-health-tips/

https://batteryuniversity.com/article/bu-409-charging-lithium-ion  



## Acknowledgements
Thanks to my wonderful wife *Ileana* for being my test subject and as alpha tester of my foolish experiments. 💙  
And a big thank you to the little hacker *AndrewTHEGamer* for the help provided! 😎😁
