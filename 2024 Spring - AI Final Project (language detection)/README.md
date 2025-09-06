# AI Final Project
*Team Project (3 people)*
## Intro
This project aims to use machine learning models to predict the language a given text is written in. We used **Quadratic Discriminant Analysis (QDA)**, **Support Vector Machine (SVM)**, and **Random Forest** with **voting classifier**.  

>**Dataset:**  
[Language Identification Dataset](https://huggingface.co/datasets/papluca/language-identification)  

>**Files**  
[**Report**](https://github.com/Mars-1114/cs-portfolio/blob/main/2024%20Spring%20-%20AI%20Final%20Project%20(language%20detection)/Team37_Slides.pdf)  
[**Code**](https://github.com/Mars-1114/cs-portfolio/tree/main/2024%20Spring%20-%20AI%20Final%20Project%20(language%20detection)/project)

## Goal
- Train multiple classification models to identify the language from plain texts.
- Get an accuracy of over 90%.

## Result
- **SVM** has the highest performance (acc = 0.933)
- **Bagging** can improve the accuracy by 0.02~0.04
- For shorter text length (such as a single word), our model underperforms (acc = 0.3~0.4), but **voting** helps increase the accuracy (acc = 0.42)
- **1-gram / 2-gram frequency analysis** are the most significant features in language detection.

## What I Learned
- Feature extraction on how to differentiate languages.
- Machine learning model training process.

## Improvements
- Training the model was time-consuming, a code optimization or parallel programming may reduce this problem.
- Multi-layer classification (language family -> sublanguage) may improve the performance.

## Contributions
| Member    | Research | Code   | Report |
| ------    | -------  | -----  | ------ |
| **嚴偉哲** | 90%      | 85%    | 60%    |
| **黃毓為** | 10%      | 5%     | 20%    |
| **郭穎達** | 0%       | 10%    | 20%    |