#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <Photos/Photos.h>

extern "C" {
    void SaveImageToGalleryNative(const char* path) {
        if (path == NULL) return;
        
        NSString* imagePath = [NSString stringWithUTF8String:path];
        UIImage* image = [UIImage imageWithContentsOfFile:imagePath];
        if (image == nil) {
            NSLog(@"CozyAR iOS Helper: Failed to load image from path %@", imagePath);
            return;
        }

        // Save image to Saved Photos Album
        UIImageWriteToSavedPhotosAlbum(image, nil, nil, nil);
        NSLog(@"CozyAR iOS Helper: Image save requested for %@", imagePath);
    }

    void SaveVideoToGalleryNative(const char* path) {
        if (path == NULL) return;
        
        NSString* videoPath = [NSString stringWithUTF8String:path];
        if (UIVideoAtPathIsCompatibleWithSavedPhotosAlbum(videoPath)) {
            // Save video to Saved Photos Album
            UISaveVideoAtPathToSavedPhotosAlbum(videoPath, nil, nil, nil);
            NSLog(@"CozyAR iOS Helper: Video save requested for %@", videoPath);
        } else {
            NSLog(@"CozyAR iOS Helper: Video at path %@ is not compatible with saved photos album", videoPath);
        }
    }
}
