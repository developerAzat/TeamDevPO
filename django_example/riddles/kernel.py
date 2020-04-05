# import pandas as pd
import nltk
nltk.download('punkt')
nltk.download('wordnet')
nltk.download("stopwords")

# from nltk.stem import WordNetLemmatizer
# lemmatizer = WordNetLemmatizer()
import pickle
import numpy as np
# from keras.models import Sequential
# from keras.layers import Dense, Activation, Dropout
# from keras.optimizers import SGD
import random
import json

# from nltk.corpus import stopwords
from pymystem3 import Mystem
# from string import punctuation

from keras.models import load_model





with open('JSONforLemm.txt', encoding='UTF-8') as json_file:
    intents = json.load(json_file)
    
words = pickle.load(open('words.pkl','rb'))
classes = pickle.load(open('classes.pkl','rb'))
model = load_model('chatbot_model.h5')



def clean_up_sentence(sentence):
    mystem = Mystem()
    sentence = str(sentence)
    sentence_words = nltk.word_tokenize(sentence, 'russian')
    sentence_words = [mystem.lemmatize(wor.lower())[0] for wor in sentence_words]
    return sentence_words

# return bag of words array: 0 or 1 for each word in the bag that exists in the sentence

def bow(sentence, words, show_details=True):
    # tokenize the pattern
    sentence_words = clean_up_sentence(sentence)
    # bag of words - matrix of N words, vocabulary matrix
    bag = [0]*len(words)
    for s in sentence_words:
        for i,w in enumerate(words):
            if w == s:
                # assign 1 if current word is in the vocabulary position
                bag[i] = 1
                if show_details:
                    print ("found in bag: %s" % w)
    return(np.array(bag))

def predict_class(sentence, model):
    # filter out predictions below a threshold
    p = bow(sentence, words,show_details=False)
    res = model.predict(np.array([p]))[0]
    ERROR_THRESHOLD = 0.25
    results = [[i,r] for i,r in enumerate(res) if r>ERROR_THRESHOLD]
    # sort by strength of probability
    results.sort(key=lambda x: x[1], reverse=True)
    return_list = []
    for r in results:
        return_list.append({"intent": classes[r[0]], "probability": str(r[1])})
    return return_list

def getResponse(ints, intents_json):
    tag = ints[0]['intent']
    list_of_intents = intents_json['intents']
    for i in list_of_intents:
        if(i['tag']== tag):
            result = random.choice(i['responses'])
            break
    return result

def returnResult(message):
    try:
        ints = predict_class(str(message), model)
        res = getResponse(ints, intents)
    except IndexError:
        res = "Извините, заполните обращение по ссылке https://clck.ru/MAvDL, и вам обязательно ответят!"
    return res

# print(returnResult('как подписание оферту'))
with open('result.txt', 'w', encoding='UTF-8') as f:
    f.write(returnResult('sdsd'))