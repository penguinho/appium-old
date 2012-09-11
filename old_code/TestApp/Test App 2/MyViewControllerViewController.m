//
//  MyViewControllerViewController.m
//  Test App 2
//
//  Created by Joseph Cuellar on 8/1/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import "MyViewControllerViewController.h"

@interface MyViewControllerViewController ()

@end

@implementation MyViewControllerViewController
@synthesize answerLabel;
@synthesize firstArg;
@synthesize secondArg;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
	[firstArg setAccessibilityLabel:@"IntegerA"];
	[secondArg setAccessibilityLabel:@"IntegerB"];
	[answerLabel setAccessibilityLabel:@"Compute Sum"];
	
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    // Do any additional setup after loading the view from its nib.
}

- (void)viewDidUnload
{
    [self setFirstArg:nil];
    [self setSecondArg:nil];
    [self setAnswerLabel:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}

- (void)dealloc {
    [firstArg release];
    [secondArg release];
    [answerLabel release];
    [super dealloc];
}
- (IBAction)computeAction:(id)sender {
	int a = [[firstArg text] intValue];
	int b = [[secondArg text] intValue];
	int sum = a + b;
	NSString *newLabelValue = [NSString stringWithFormat:@"%d",sum];
	[answerLabel setText:newLabelValue];
}
@end
