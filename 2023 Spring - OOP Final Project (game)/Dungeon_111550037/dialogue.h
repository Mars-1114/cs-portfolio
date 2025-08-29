#ifndef _DIALOGUE
#define _DIALOGUE

#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include<vector>
using namespace std;

class Dialogue {
private:
	int line;
	string script;

public:
	Dialogue() = default;
	Dialogue(int id, string s) {
		line = id;
		script = s;
	}
	string GetLine() {
		return script;
	}
	int getID() {
		return line;
	}
};

//create the script
vector<Dialogue> addScript();
//print the dialogue
void printLine(vector<Dialogue>, int, int = 0);
void printLine(string, int = 0);

#endif