#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <string.h>
#include <vector>
#include <conio.h>
#include <random>
#include "dialogue.h"
#include "Dungeon.h"
#include "room.h"
#include "entity.h"
#include "npc.h"

extern vector<Enemy*> enemyLoad;
extern vector<Item*> itemLoad;
extern NPC* npcLoad;
extern vector<List> enemy, item, bp, list;
extern bool hasStayed;
extern int endGameID;
bool visited;
bool r9_DialogueTrigger = false;
bool r15_DialogueTrigger = false;

vector<int> roomVisited;

void Dungeon::eventCheck() {
	item.clear();
	enemy.clear();
	bp.clear();
	itemLoad.clear();
	enemyLoad.clear();
	npcLoad = NULL;
	srand(time(NULL));
	for (int i = 0; i < enemyList.size(); i++) {
		enemyList[i].loadCheck(player.getRoomID());
	}
	for (int i = 0; i < itemList.size(); i++) {
		itemList[i].loadCheck(player.getRoomID());
	}
	for (int i = 0; i < npcList.size(); i++) {
		npcList[i].loadCheck(player.getRoomID());
	}
	visited = false;
	for (int i = 0; i < roomVisited.size(); i++) {
		if (roomVisited[i] == player.getRoomID()) {
			visited = true;
		}
	}
	if (!visited) {
		roomVisited.push_back(player.getRoomID());
	}
	if (!hasStayed) {
		//has enemy?
		if (enemyLoad.size() != 0) {
			if (player.getRoomID() != 16) {
				if (!visited) {
					printLine(script, rand() % 5 + 8, 1);
					printLine("(It seems like a fight is inevitable)");
				}
				else {
					printLine(script, rand() % 2 + 13);
				}
			}
		}
		if (player.getRoomID() == 6 && !visited) {
			printLine(script, 20);
			printLine(script, 21);
		}
		if (player.getRoomID() == 16 && !visited) {
			for (int i = 0; i < 5; i++) {
				printLine(script, 30 + i);
			}
		}
	}
}

void Dungeon::lockedMsg(int room) {
	if (room == 2) {
		printLine(script, 16);
	}
	else if (room == 9) {
		if (!r9_DialogueTrigger) {
			printLine(script, 17);
			printLine(script, 18);
			printLine(script, 51);
			printLine(script, 52);
			printLine(script, 53);
			r9_DialogueTrigger = true;
		}
		else {
			printLine(script, 19);
		}
	}
	else if (room == 15) {
		if (!r15_DialogueTrigger) {
			printLine(script, 22);
			printLine(script, 23);
			printLine(script, 24);
			r15_DialogueTrigger = true;
		}
		vector<int> id = { 0, 1 };
		vector<List> temp = {
			{ 0, "yes" },
			{ 1, "no" }
		};
		printScreen(script[25].GetLine(), id, temp, &Dungeon::unlockDoor);
	}
	else if (room == 16) {
		printLine("(There is no way back.)");
	}
	else if (room == 0) {
		printLine("The Gatekeeper: Unfortunately, the entrance is locked. For now.");
	}
	else if (room == 100) {
		endGameID = 2;
	}
	else if (room == 115) {
		printLine(script, 27);
		printLine(script, 28);
		printLine(script, 29);
		printLine(script, 49);
		Dungeon::room[15].unlock(1);
		Dungeon::room[0].unlock(0);
	}
}

void Dungeon::unlockDoor(int n) {
	if (n == 0) {
		if (player.getRoomID() == 15) {
			if (player.getCoin() >= 50) {
				room[15].unlock(0);
				room[2].unlock(0);
				player.updateStatus(0, 0, 0, -50);
				printLine(script, 26);
			}
			else {
				printLine(script, 50);
			}
		}
		if (player.getRoomID() == 9) {
			if (r9_DialogueTrigger) {
				printLine(script, 47);
				printLine(script, 48);
			}
			else {
				printLine(script, 54);
				printLine(script, 55);
			}
			room[9].unlock(0);
		}
	}
}