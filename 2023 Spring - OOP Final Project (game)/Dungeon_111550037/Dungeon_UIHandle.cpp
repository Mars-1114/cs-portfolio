#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <string.h>
#include <vector>
#include <conio.h>
#include "dialogue.h"
#include "Dungeon.h"
#include "room.h"
#include "entity.h"

extern vector<Enemy*> enemyLoad;
extern vector<Item*> itemLoad;
extern NPC* npcLoad;

void Dungeon::printScreen(string title, vector<int> actionList, vector<List> searchPool, void (Dungeon::* func)(int), int mode) {
	bool isSelected = false;
	int curPos = 0;
	int listLen = (int)size(actionList);
	char input = ' ';
	while (!isSelected) {
		cout << title << endl << endl;
		if (mode == 3) { //backpack
			if ((int)actionList.size() == 1) {
				cout << "(empty)" << endl;
			}
		}
		for (int i = 0; i < listLen; i++) {
			if (curPos == i) {
				cout << "> " << searchPool[actionList[i]].name;
			}
			else {
				cout << "  " << searchPool[actionList[i]].name;
			}
			if (mode == -1) { //general
				if (actionList[i] == 10) {
					cout << " with " << (*npcLoad).getName();
				}
			}
			else if (mode == 3) { //backpack
				if (searchPool[i].name != "cancel") {
					if (player.getArmor() == &((*player.getBackpack())[i]) || player.getWeapon() == &((*player.getBackpack())[i])) {
						cout << " (equipped)";
					}
				}
			}
			cout << endl;
		}
		if (mode == 1) { //enemy
			if (searchPool[curPos].name != "cancel") {
				enemyLoad[curPos]->showStats();
			}
		}
		else if (mode == 2) { //item
			if (searchPool[curPos].name != "cancel") {
				itemLoad[curPos]->showStats();
			}
		}
		else if (mode == 3) { //backpack
			if (searchPool[curPos].name != "cancel") {
				(*player.getBackpack())[curPos].showStats();
				if (((*player.getBackpack())[curPos]).getEquippable() == 3) {
					cout << endl << "(press C to eat)";
				}
				else if (player.getArmor() != &((*player.getBackpack())[curPos]) && player.getWeapon() != &((*player.getBackpack())[curPos])) {
					if (((*player.getBackpack())[curPos]).getEquippable() != 0) {
						cout << endl << "(press C to equip)";
					}
				}
			}
		}
		else if (mode == 4) { //occupation
			occDesc(curPos);
		}
		else if (mode == -2) { //main menu
			cout << endl << "(W, S to select, C to confirm)";
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
	if (title == "Movement" && room[player.getRoomID()].getLockDir() == (actionList[curPos] - 1)) {
		lockedMsg(player.getRoomID());
	}
	else if (title == "Movement" && room[player.getRoomID()].getTriggerDir() == (actionList[curPos] - 1)) {
		lockedMsg(player.getRoomID() + 100);
	}
	else if (func != NULL) {
		(this->*func)(actionList[curPos]);
	}
}