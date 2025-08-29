#ifndef _DUNGEON
#define _DUNGEON

#include <stdio.h>
#include <iostream>
#include <stdlib.h>
#include <string.h>
#include "dialogue.h"
#include "player.h"
#include "room.h"
#include "enemy.h"
#include "item.h"
#include "npc.h"
using namespace std;

struct List {
	int id;
	string name;
};

class Dungeon {
private:
	Player player;
	Room* room;
	vector<Enemy> enemyList;
	vector<Item> itemList;
	vector<NPC> npcList;
	vector<Dialogue> script;
public:
	Dungeon() = default;
	//start the game and control when the game ends
	void RunGame();
	//create a player
	Player createPlayer(string, int);
	//initialize game
	void startGame();
	//print and handle the action list
	void actionList(int);
	//print the action
	void printScreen(string, vector<int>, vector<List>, void (Dungeon::* func)(int), int mode = 0);
	//execute the action
	void runAction(int);
	//handle anything related to npc
	void npcAction(int);
	//decide who to attack
	void attack(int);
	//choose what to pick up
	void pickup(int);
	//choose what to drop
	void drop(int);
	//quite literally
	void equip(int);
	//
	void unlockDoor(int);
	//choose the occupation
	void selectOcc(int);
	//decide what would happen when enter a room
	void eventCheck();
	//loot the enemy (kinda nsfw if u think about it)
	void lootDrop(Enemy);
	//print locked message
	void lockedMsg(int);
	//ending
	void endingCutscene();
	friend void npcSell(int);
	friend void Item::showStats();
	friend void attackEnemy(int, int, vector<int>&);
	friend void npcPrintScreen(string, vector<int>, vector<List>, void (*func)(int), int);
};

void generateAction();
void searchDelete(Enemy*, vector<Enemy>&);
void searchDelete(Item*, vector<Item>&);
void searchDelete(sell*, vector<sell>&);

#endif
