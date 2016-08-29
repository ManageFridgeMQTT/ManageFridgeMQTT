function [ Result, ConfidentValue ] = AnalysisStandardOrNot( CheckedFileName, SrcFileName, Threshold, ViewResult )

%     SrcRgbImage = imread(SrcFileName); 
%     CheckedRgbImage = imread(CheckedFileName);
    
%     correlationOutput = normxcorr2(SrcRgbImage(:,:,1), CheckedRgbImage(:,:,1));
%     [maxCorrValue, maxIndex] = max(abs(correlationOutput(:)));
    
%     Threshold = 0.75;
    [Result, Reliability_Rate] = CheckingMatchingImg(CheckedFileName, SrcFileName);

    if(Reliability_Rate > Threshold) 
        Result = 1;
    else
        Result = 0;
    end
    ConfidentValue = Reliability_Rate;
    
%     if(ViewResult == 1)
%         [ypeak, xpeak] = ind2sub(size(correlationOutput),maxIndex(1));
%         corr_offset = [(xpeak-size(SrcRgbImage,2)) (ypeak-size(SrcRgbImage,1))];
%         
%         subplot(2,2,1);
%         imshow(SrcRgbImage);
%         subplot(2,2,2);
%         imshow(correlationOutput,[]);
%         title(num2str(maxCorrValue))
%         subplot(2,2,3);
%         imshow(CheckedRgbImage);
%         hold on;
%         rectangle('position',[corr_offset(1) corr_offset(2) size(SrcRgbImage,2) size(SrcRgbImage,1)],...
%                   'edgecolor','g','linewidth',2);
%         title(num2str(maxCorrValue))
%         subplot(2,2,4);
%         plot(abs(correlationOutput(:)));
%     end
end

