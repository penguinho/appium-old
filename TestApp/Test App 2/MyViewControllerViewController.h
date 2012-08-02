//
//  MyViewControllerViewController.h
//  Test App 2
//
//  Created by Joseph Cuellar on 8/1/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface MyViewControllerViewController : UIViewController
@property (retain, nonatomic) IBOutlet UITextField *firstArg;
@property (retain, nonatomic) IBOutlet UITextField *secondArg;
@property (retain, nonatomic) IBOutlet UILabel *answerLabel;

- (IBAction)computeAction:(id)sender;

@end
