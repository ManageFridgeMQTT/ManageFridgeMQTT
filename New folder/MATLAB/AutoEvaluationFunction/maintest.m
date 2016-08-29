clc; clear

ProjectPath = 'D:\\Matlab_ChamTuDong\\AutoEvaluationApplicationExample\\AutoEvaluationApplicationExample\\';
matlabFuncPath = [ ProjectPath 'AutoEvaluationFunction'];
ItemImagePath = [ProjectPath 'AutoEvaluationData\\TH Milk Sample.jpg'];
ComparedImagePath = [ProjectPath 'AutoEvaluationData\\THMILKImages\\10000289_SM000965_C000008965_1453699434352.jpg'];
InputImagePath = [ProjectPath 'AutoEvaluationData\\THMILKImages\\10000319_SM000740_C000119428_1453604459724.jpg'];
BagPath = [ProjectPath 'AutoEvaluationData\\Bag.mat'];
MeanMhistRGBPath = [ProjectPath 'AutoEvaluationData\\MeanMhistRGB.mat'];
VLFeat_LibPath = [ProjectPath 'AutoEvaluationData\\VLFeat_Lib\\toolbox\\vl_setup.m'];

RealFakeThreshold = 100;
StandardOrNotThreshold = 100;
PassOrNotThreshold = 80;

[ RealOrFakeResult, StandardOrNotResult, PassOrNotResult, PercentFake, NumCorrelation, NumFeature, RealOrFakeTime, StandardOrNotTime, PassOrNotTime,  ImageStatus ] = AutoImageEvaluation( InputImagePath, ComparedImagePath, ItemImagePath, RealFakeThreshold, StandardOrNotThreshold, PassOrNotThreshold, BagPath, MeanMhistRGBPath, VLFeat_LibPath);
        
RealOrFakeResult, StandardOrNotResult, PassOrNotResult, PercentFake, NumCorrelation, NumFeature, RealOrFakeTime, StandardOrNotTime, PassOrNotTime,  ImageStatus