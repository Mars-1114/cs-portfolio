#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <string.h>
#include <vector>
#include <conio.h>
#include "dialogue.h"
#include "Dungeon.h"
#include "room.h"

extern NPC* npcLoad;
extern Dungeon dungeon;
//find the dialogue by the assigned id, returns the vector index
int findDialogue(int);
//print the action
void npcPrintScreen(string, vector<int>, vector<List>, void (*func)(int), int mode = 0);
void npcRunAction(int);
//trading action
void npcSell(int);
void npcChat(int);

vector<List> npcActionList = {
	{0, "Trade"},
	{1, "Talk"},
	{2, "Leave"}
};

void Dungeon::npcAction(int n) {
	switch (n) {
	case 0:  //general
	{
		vector<int> id;
		if (npcLoad->getShelf()->size() != 0) {
			id.push_back(0);
		}
		if (npcLoad->getDialogue().size() != 0) {
			id.push_back(1);
		}
		id.push_back(2);
		npcPrintScreen(npcLoad->getDialogue()[findDialogue(0)].GetLine(), id, npcActionList, &npcRunAction);
		break;
	}
	case 1: //trade
	{
		vector<int> id;
		vector<List> shelf;
		for (int i = 0; i < npcLoad->getShelf()->size(); i++) {
			id.push_back(i);
			shelf.push_back({ i, (*npcLoad->getShelf())[i].item.getName() });
		}
		id.push_back((int)shelf.size());
		shelf.push_back({ (int)shelf.size(), "cancel" });
		npcPrintScreen("", id, shelf, &npcSell, 1);
		break;
	}
	case 2: //chat
	{
		vector<int> id;
		vector<List> line;
		int i = 5;
		while (findDialogue(i) != -1) {
			id.push_back(i - 5);
			line.push_back({ i, npcLoad->getDialogue()[findDialogue(i)].GetLine() });
			i++;
		}
		npcPrintScreen(npcLoad->getDialogue()[findDialogue(2)].GetLine(), id, line, &npcChat);
		break;
	}
	default:
		break;
	}
}

void npcRunAction(int action) {
	switch (action) {
	case 0: //Trade
		dungeon.npcAction(1);
		break;
	case 1: //Chat
		dungeon.npcAction(2);
		break;
	case 2: //Leave
		printLine(npcLoad->getName() + ": " + npcLoad->getDialogue()[findDialogue(100)].GetLine());
		break;
	default:
		break;
	}
}

void npcSell(int item) {
	if (item != (*npcLoad->getShelf()).size()) {
		sell tempItem = (*npcLoad->getShelf())[item];
		vector<Dialogue> script = npcLoad->getDialogue();
		if (dungeon.player.getCoin() < tempItem.price) {
			printLine(npcLoad->getName() + ": " + script[findDialogue(21)].GetLine());
		}
		else {
			dungeon.player.addItem(tempItem.item);
			dungeon.player.updateStatus(0, 0, 0, -tempItem.price);
			if (!tempItem.permanent) {
				searchDelete(&tempItem, *npcLoad->getShelf());
			}
			if (tempItem.id >= 11 && tempItem.id <= 13) {
				printLine(npcLoad->getName() + ": " + script[findDialogue(20)].GetLine(), 2);
				//check if match
				if (tempItem.id == dungeon.player.getOcc() + 11) {
					printLine(script[findDialogue(23)].GetLine());
				}
				else {
					printLine(script[findDialogue(22)].GetLine());
				}
			}
			else {
				printLine(npcLoad->getName() + ": " + script[findDialogue(20)].GetLine());
			}
		}
	}
	dungeon.npcAction(0);
}

void npcChat(int id) {
	int line = (id + 3) * 10;
	while (findDialogue(line) != -1) {
		printLine(npcLoad->getName() + ": " + npcLoad->getDialogue()[findDialogue(line)].GetLine());
		line++;
	}
	dungeon.npcAction(0);
}

void npcPrintScreen(string title, vector<int> actionList, vector<List> searchPool, void (*func)(int), int mode) {
	bool isSelected = false;
	int curPos = 0;
	int listLen = (int)size(actionList);
	char input = ' ';
	if (mode == 1) { //trade
		curPos = (int)actionList.size() - 1;
		cout << title;
	}
	while (!isSelected) {
		cout << npcLoad->getName() << ": ";
		if (mode == 0) { //default
			cout << title;
		}
		else if (mode == 1) { //trade
			if (searchPool[curPos].name != "cancel") {
				cout << npcLoad->getDialogue()[findDialogue((*npcLoad->getShelf())[curPos].id)].GetLine();
			}
			else {
				cout << npcLoad->getDialogue()[findDialogue(1)].GetLine();
			}
		}
		cout << endl << endl;
		if (mode == 1) {
			cout << "[shelf]" << endl;
		}
		for (int i = 0; i < listLen; i++) {
			if (curPos == i) {
				cout << "> " << searchPool[actionList[i]].name;
			}
			else {
				cout << "  " << searchPool[actionList[i]].name;
			}
			cout << endl;
		}
		if (mode == 1) { //trade
			if (searchPool[curPos].name != "cancel") {
				cout << endl << "- " << (*npcLoad->getShelf())[curPos].price << "G\t" << "(You have " << dungeon.player.getCoin() << "G)" << endl;
				(*npcLoad->getShelf())[curPos].item.showStats();
			}
		}
		input = _getch();
		if (input == 'W' || input == 'w') {
			if (curPos == 0) {
				curPos = listLen - 1;
			}
			else {
				curPos--;
			}
		}
		else if (input == 'S' || input == 's') {
			if (curPos == listLen - 1) {
				curPos = 0;
			}
			else {
				curPos++;
			}
		}
		else if (input == 'c' || input == 'C') {
			isSelected = true;
		}
		system("cls");
		input = ' ';
	}
	(*func)(actionList[curPos]);
}

int findDialogue(int id) {
	for (int i = 0; i < npcLoad->getDialogue().size(); i++) {
		if (npcLoad->getDialogue()[i].getID() == id) {
			return i;
		}
	}
	return -1;
}