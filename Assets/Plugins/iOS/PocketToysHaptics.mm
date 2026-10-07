#import <UIKit/UIKit.h>

extern "C" void PocketToysHaptic(int kind)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 10.0, *)) {
            if (kind == 2) {
                UINotificationFeedbackGenerator *feedback = [[UINotificationFeedbackGenerator alloc] init];
                [feedback notificationOccurred:UINotificationFeedbackTypeSuccess];
                #if !__has_feature(objc_arc)
                [feedback release];
                #endif
            } else {
                UIImpactFeedbackGenerator *feedback = [[UIImpactFeedbackGenerator alloc] initWithStyle:(kind == 1 ? UIImpactFeedbackStyleMedium : UIImpactFeedbackStyleLight)];
                [feedback impactOccurred];
                #if !__has_feature(objc_arc)
                [feedback release];
                #endif
            }
        }
    });
}
