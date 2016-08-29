function [ RealOrFakeResult, StandardOrNotResult, PassOrNotResult, PercentFake, NumCorrelation, NumFeature, RealOrFakeTime, StandardOrNotTime, PassOrNotTime,  ImageStatus ] = AutoRunEvaluation( InputImagePath, ComparedImagePath, ItemImagePath, RealFakeThreshold, StandardOrNotThreshold, PassOrNotThreshold, ProcessType,  BagPath, MeanMhistRGBPath, VLFeat_LibPath)
%AUTOIMAGEEVALUATION Summary of this function goes here
%   Detailed explanation goes here
%   ProcessType = 0 : All, 1 : Algorithm 1, 2 : Algorithm 2, 3 : Algorithm 3 
    InstallVLFLib(VLFeat_LibPath);
    
    RealOrFakeResult = 0;
    StandardOrNotResult = 0;
    PassOrNotResult = 0;
    PercentFake = 0; 
    NumCorrelation = 0;
    NumFeature = 0;
    RealOrFakeTime = 0;
    StandardOrNotTime = 0;
    PassOrNotTime = 0;
    ImageStatus = '';
%     Check Image Real Or Fake
    if( ProcessType == 0 || ProcessType == 1)
        hasInputImage = 1;
        try
            InputImage = imread(InputImagePath);

             tic 
             [RealOrFakeResult, PercentReal, PercentFake] = AnalysisRealOrFake(InputImagePath, BagPath, MeanMhistRGBPath);
             RealOrFakeTime  = toc;

            if(PercentFake >= RealFakeThreshold)
                RealOrFakeResult = 0;
            else 
                RealOrFakeResult = 1;
            end
        catch ME
            hasInputImage = 0;
            RealOrFakeResult = 0;
            PercentFake = -1;
            ImageStatus = [ImageStatus '1-' ME.message];        
        end
    end
    
%     if (RealOrFakeResult == 0)
%         StandardOrNotResult = 0;
%         PassOrNotResult = 0;
%         NumCorrelation = 0;
%         NumFeature = 0;
%         return
%     end
        
  % Check Image Standard or Not (means that the image is captured at the same place of ComparedImage)
    if( ProcessType == 0 || ProcessType == 2)
        hasComparedImage = 1;
        try
            ComparedImage = imread(ComparedImagePath);

            tic
            [ StandardOrNotResult, NumCorrelation ] = AnalysisStandardOrNot( InputImagePath, ComparedImagePath, StandardOrNotThreshold, 0);
            StandardOrNotTime = toc;

            if(NumCorrelation > StandardOrNotThreshold)
                StandardOrNotResult = 1;
            else 
                StandardOrNotResult = 0;
            end
        catch ME 
            hasComparedImage = 0;
            StandardOrNotResult = 0;
            NumCorrelation = 0;
            ImageStatus = [ImageStatus '2-' ME.message];    
        end
    end
%     if(StandardOrNotResult == 0)
%         PassOrNotResult = 0;
%         NumFeature = 0;
%         return;
%     end
    
    % Check image Passed or not (means that item exits in image)
    if( ProcessType == 0 || ProcessType == 3)
        hasItemImage = 1;
        try
            ItemImage = imread(ItemImagePath);

            tic
            [ PassOrNotResult, NumFeature ] = AnalysisPassOrNot( InputImagePath, ItemImagePath, PassOrNotThreshold, 0  );
            PassOrNotTime = toc;

            if(NumFeature >= PassOrNotThreshold)
                PassOrNotResult = 1;
            else 
                PassOrNotResult = 0;
            end
        catch ME
            hasItemImage = 0;
            PassOrNotResult = 0;
            NumFeature = 0;
            ImageStatus = [ImageStatus '3-' ME.message];    
        end
    end
    
%     if (ImageStatus == ' ')
%         ImageStatus = 'No Error';
%     end
%     if(PassOrNotResult == 0)
%         return;
%     end
end

