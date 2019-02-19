# README

## Brave New Word

The game is a simple word game where you must select adjacent (including diagonal!) tiles to create words. Rarer words generate more points. The frequency of the words was calculated using data collected from Reddit comments.

## Local Install

1. [Install Unity](https://unity3d.com/).
2. Download or clone this repo.
3. Open Unity, and open this project by navigating to where you downloaded this repo.
    - You may need to re-download the appropriate version of the Google Firebase SDK for Unity and re-import it into the Unity project. If it still does not work, make sure to try running Assets->Play Services Resolver->Android Resolver->Resolve.
    - If you wish to connect to Firebase from the Unity Editor you will also have to download the appropriate .p12 file for authentication (contact me if you don't have access) and place it anywhere within the /Assets directory. 
4. Attach a mobile device to your computer and use the UnityRemote app to run the game from the editor, or create a build of the game for your specific device.

## Research

This game is being used as part of an on-going research project at UW-Madison studying frictional design patterns and its impact on Player Experience. Visit the [Complex Play Lab website](http://complexplay.org/) for more info.

## Building for Android

Download build-tools for v7.0 (Nougat), the correct sdk version from this link: https://answers.unity.com/questions/1320634/unable-to-list-target-platforms-when-i-try-to-buil.html and download the correct version of the NDK from Unity -> Preferences -> External Tools -> Download JDK and NDK inside.

