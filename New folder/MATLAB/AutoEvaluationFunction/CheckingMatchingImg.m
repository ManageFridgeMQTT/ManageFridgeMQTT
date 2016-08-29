function [Result, Reliability_Rate] = CheckingMatchingImg(lImage_1, lImage_2)
     %% ==============================================================================
     % Author: Ung Quang Huy
     % This function checks correlation of two images using SIFT features 
     % and VLFeat Library (open source code).
     % http://www.vlfeat.org
     % Input: 
     % + lImage_1: link of the 1st image.
     % + lImage_2: link of the second image.
     % + Threshold: First threshold
     % + VLThreshold: Threshold in vl_ubcmatch function.
     % Output:
     % + Result: [1] Matching, [0] Unknown.
     % + Reliability_Rate: Reliability rate of final result.
     %% ==============================================================================
     % Initialize threshold and read image.
     
     Threshold = 50;
     VLThreshold = 2;
     UpBoundary = 187;
     Image_1 = imread(lImage_1);
     Image_2 = imread(lImage_2);
     
     % Rotaion Image if size of two images are difference.
     if size(Image_1, 1) ~= 640
         Image_1 = imrotate(Image_1, -90, 'loose');
     end

     if size(Image_2, 1) ~= 640
         Image_2 = imrotate(Image_2, -90, 'loose');
     end

     % Rezise two images.
     OI1 = imresize(Image_1, 1/2);
     OI2 = imresize(Image_2, 1/2);        

     % Transforming to gray-scale images
     I1 = single(rgb2gray(OI1));
     I2 = single(rgb2gray(OI2));

     % Extracting descriptions of two images.
     [~, description_1] = vl_sift(I1);
     [~, description_2] = vl_sift(I2);
     
     % Get the number of matching descriptions in two images.
     [Matches, ~] = vl_ubcmatch(description_1, description_2, VLThreshold);
     NumOfMatchingPoint = size(Matches, 2);
     
     % Compare to thresholds value
     if NumOfMatchingPoint > Threshold
         Result = 1;
          % Compute reliability rate
         Reliability_Rate = (NumOfMatchingPoint - Threshold) / (UpBoundary - Threshold) * 100;
     else
         Result = 0;
         % Compute reliability rate
         Reliability_Rate = (Threshold - NumOfMatchingPoint) / Threshold * 100;
     end    
end