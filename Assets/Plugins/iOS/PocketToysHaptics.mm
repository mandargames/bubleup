#import <UIKit/UIKit.h>

extern "C" void PocketToysHaptic(int kind)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 10.0, *)) {
            if (kind == 2) {
                static UINotificationFeedbackGenerator *feedback;
                if (!feedback) feedback = [[UINotificationFeedbackGenerator alloc] init];
                [feedback notificationOccurred:UINotificationFeedbackTypeSuccess];
                [feedback prepare];
            } else {
                static UIImpactFeedbackGenerator *pump;
                static UIImpactFeedbackGenerator *landing;
                if (!landing) landing = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                if (!pump) {
                    if (@available(iOS 13.0, *)) pump = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleSoft];
                    else pump = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                }
                UIImpactFeedbackGenerator *feedback = kind == 1 ? landing : pump;
                if (@available(iOS 13.0, *)) [feedback impactOccurredWithIntensity:(kind == 1 ? 0.6 : 0.35)];
                else [feedback impactOccurred];
                [feedback prepare];
            }
        }
    });
}
