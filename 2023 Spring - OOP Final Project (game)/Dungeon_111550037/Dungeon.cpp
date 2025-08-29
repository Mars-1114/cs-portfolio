#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <string.h>
#include <vector>
#include <conio.h>
#include "dialogue.h"
#include "Dungeon.h"
#include "room.h"
#include "npc.h"

extern Dungeon dungeon;
vector<Enemy*> enemyLoad;
vector<Item*> itemLoad;
NPC* npcLoad;
vector<List> enemy, item, bp, list;
bool hasStayed = false;
int endGameID = 0;
void attackEnemy(int, int, vector<int>&);
void sort(vector<int>&);

void Dungeon::RunGame() {
	generateAction();
	script = addScript();
	actionList(0);
	while (endGameID == 0) {
		eventCheck();
		hasStayed = true;
		actionList(2);
	}
	endingCutscene();
}

Player Dungeon::createPlayer(string name, int occ) {
	Item tempMain, tempArmor;
	Player p;
	if (occ == 0) {
		p = Player(name, 0, 0, 100);
		tempMain = Item("Basic Sword", 0, 0, 40, 0, 0, 1);
		tempArmor = Item("Shield", 0, 0, 0, 30, 0, 2);
	}
	else if (occ == 1) {
		p = Player(name, 0, 1, 100);
		tempMain = Item("Basic Bow", 0, 0, 45, 0, 0, 1);
		tempArmor = Item("Leather Armor", 0, 0, 0, 25, 0, 2);
	}
	else if (occ == 2) {
		p = Player(name, 0, 2, 80);
		tempMain = Item("Basic Wand", 0, 0, 40, 0, 0, 1);
		tempArmor = Item("Cape", 0, 0, 0, 20, 0, 2);
	}
	p.addItem(tempMain);
	p.addItem(tempArmor);
	p.setWeapon((*p.getBackpack())[0]);
	p.setArmor((*p.getBackpack())[1]);
	return p;
}

void Dungeon::startGame() {
	string tempName;
	int tempOcc;
	for (int i = 0; i < 5; i++) {
		printLine(script, i);
	}
	printLine(script, 5, 1);
	cout << endl << "> ";
	cin >> tempName;
	system("cls");
	actionList(3);
	tempOcc = player.getOcc();
	player = createPlayer(tempName, tempOcc);
	printLine(script, 7);
}

void Dungeon::actionList(int type) {
	switch (type) {
	case 0: //menu screen
	{
		vector<int> listID = { 0 };
		printScreen("THE DUNGEON", listID, list, &Dungeon::runAction, -2);
		break;
	}
	case 1: //movement
	{
		vector<int> dir;
		for (int i = 1; i <= 4; i++) {
			if (room[player.getRoomID()].getRoom(i) != -1) {
				dir.push_back(i + 1);
			}
		}
		player.setPrevRoom(player.getRoomID());
		hasStayed = false;
		printScreen("Movement", dir, list, &Dungeon::runAction);
		break;
	}
	case 2: //general
	{
		vector<int> listID = { 7, 12 };
		if (enemyLoad.size() != 0) {
			listID.push_back(8);
			listID.push_back(9);
		}
		else {
			if (itemLoad.size() != 0) {
				listID.push_back(11);
			}
			if (npcLoad != NULL) {
				listID.push_back(10);
			}
			listID.push_back(6);
		}
		printScreen("Select", listID, list, &Dungeon::runAction, -1);
		break;
	}
	case 3: //occupation selection
	{
		vector<int> occID = {0, 1, 2};
		vector<List> occName = {
			{0, "Knight"},
			{1, "Archer"},
			{2, "Magus"}
		};
		printScreen(script[6].GetLine(), occID, occName, &Dungeon::selectOcc, 4);
		break;
	}
	case 4: //choose who to attack
	{
		enemy.clear();
		vector<int> enemyID;
		for (int i = 0; i < enemyLoad.size(); i++) {
			enemyID.push_back(i);
			enemy.push_back({ i, enemyLoad[i]->getName() });
		}
		enemy.push_back({ (int)enemyID.size(), "cancel" });
		enemyID.push_back((int)enemyID.size());
		printScreen("Choose target", enemyID, enemy, &Dungeon::attack, 1);
		break;
	}
	case 5: //choose what to collect
	{
		item.clear();
		vector<int> itemID;
		for (int i = 0; i < itemLoad.size(); i++) {
			itemID.push_back(i);
			item.push_back({ i, itemLoad[i]->getName() });
		}
		item.push_back({ (int)itemID.size(), "cancel" });
		itemID.push_back((int)itemID.size());
		printScreen("Collect", itemID, item, &Dungeon::pickup, 2);
		break;
	}
	case 6: //inspect & use item
	{
		bp.clear();
		vector<int> bpID;
		for (int i = 0; i < (*player.getBackpack()).size(); i++) {
			bpID.push_back(i);
			bp.push_back({ i, (*player.getBackpack())[i].getName() });
		}
		bp.push_back({ (int)bpID.size(), "cancel" });
		bpID.push_back((int)bpID.size());
		printScreen("Backpack", bpID, bp, &Dungeon::equip, 3);
		break;
	}
	default:
		break;
	}
}

void Dungeon::runAction(int actionID) {
	switch (actionID) {
	case 0: //New Game
		startGame();
		room = Generate();
		enemyList = enemySummon();
		itemList = itemSummon();
		npcList = npcSummon();
		break;
	case 1: //Load Game

		break;
	case 2: //Move North
		player.setRoomID(room[player.getRoomID()].getRoom(1));
		break;
	case 3: //Move South
		player.setRoomID(room[player.getRoomID()].getRoom(2));
		break;
	case 4: //Move East
		player.setRoomID(room[player.getRoomID()].getRoom(3));
		break;
	case 5: //Move West
		player.setRoomID(room[player.getRoomID()].getRoom(4));
		break;
	case 6: //Move
		actionList(1);
		break;
	case 7: //Check Status
	{
		char input = ' ';
		player.showStats();
		cout << "(press C to exit)";
		while (input != 'c' && input != 'C') {
			input = _getch();
		}
		system("cls");
		break;
	}
	case 8: //Attack Target
		actionList(4);
		break;
	case 9: //Retreat
		if (player.getRoomID() != 16) {
			for (int i = 1; i <= 4; i++) {
				if (room[player.getRoomID()].getRoom(i) == player.getPrevRoom()) {
					runAction(i + 1);
					printLine(script, 15);
					break;
				}
			}
		}
		else {
			lockedMsg(16);
		}
		break;
	case 10: //Talk
		if (player.getRoomID() == 9) {
			bool boxGet = false;
			for (int i = 0; i < player.getBackpack()->size(); i++) {
				if ((*player.getBackpack())[i].getName() == "Old Toolbox") {
					boxGet = true;
					player.removeItem(i);
					break;
				}
			}
			if (boxGet) {
				unlockDoor(0);
			}
		}
		npcAction(0);
		break;
	case 11: //Open Chest
		actionList(5);
		break;
	case 12: //Open Backpack
		actionList(6);
		break;
	default:
		break;
	}
}

void Dungeon::attack(int target) {
	vector<int> dead;
	if (target != enemyLoad.size()) {
		//player attack
		attackEnemy(target, player.getATK(), dead);
		if (player.getOcc() == 2) { //magus
			for (int i = 0; i < enemyLoad.size(); i++) {
				if (i != target) {
					attackEnemy(i, (int)(0.75 * (float)player.getATK()), dead);
				}
			}
		}
		//refresh
		sort(dead);
		for (int i = 0; i < dead.size(); i++) {
			searchDelete(enemyLoad[dead[i]], enemyList);
		}
		enemyLoad.clear();
		for (int i = 0; i < enemyList.size(); i++) {
			enemyList[i].loadCheck(player.getRoomID());
		}
		//enemy attack
		if (enemyLoad.size() == 0) {
			if (player.getRoomID() != 16) {
				printLine("(You have cleared this room)");
			}
			else {
				endGameID = 3;
			}
		}
		else {
			for (int i = 0; i < enemyLoad.size(); i++) {
				printLine("enemy " + to_string(i + 1) + "/" + to_string(enemyLoad.size()) + "\n", 1);
				player.takeDamage(enemyLoad[i]->getATK());
				int netDMG = enemyLoad[i]->getATK() - player.getDEF();
				if (netDMG < 0) {
					netDMG = 0;
				}
				enemyLoad[i]->attackMessage(netDMG, "player");
				if (player.isDead()) {
					printLine("(You lost the fight.)");
					endGameID = 1;
					break;
				}
				else {
					printLine("(You have " + to_string(player.getCurHP()) + " HP left)");
				}
			}
		}
	}
}

void Dungeon::pickup(int target) {
	if (target != itemLoad.size()) {
		player.addItem(*itemLoad[target]);
		searchDelete(itemLoad[target], itemList);
	}
}

void Dungeon::drop(int target) {
	if (target != (*player.getBackpack()).size()) {
		Item temp = (*player.getBackpack())[target];
		temp.setRoomID(player.getRoomID());
		itemList.push_back(temp);
		player.removeItem(target);
	}
}

void Dungeon::equip(int target) {
	if (target != (*player.getBackpack()).size()) {
		if ((*player.getBackpack())[target].getEquippable() == 1) {
			player.setWeapon((*player.getBackpack())[target]);
		}
		else if ((*player.getBackpack())[target].getEquippable() == 2) {
			player.setArmor((*player.getBackpack())[target]);
		}
		else if ((*player.getBackpack())[target].getEquippable() == 3) {
			player.setCurHP(player.getCurHP() + (*player.getBackpack())[target].getHP());
			player.removeItem(target);
			if (player.getCurHP() == player.getMaxHP()) {
				printLine("(Your health has fully restored)");
			}
			else {
				printLine("(Your health has increased to " + to_string(player.getCurHP()) + ")");
			}
		}
	}
}

void Dungeon::selectOcc(int n) {
	player = createPlayer("", n);
}

void Dungeon::lootDrop(Enemy target) {
	srand(time(NULL));
	int scale = target.getLvl();
	int coinLoot = 10 * scale + rand() % 4 - 1;
	int expLoot = 6 * scale + rand() % 3 - 1;
	printLine("(You gained " + to_string(coinLoot) + "G and " + to_string(expLoot) + " exp)");
	player.updateStatus(0, 0, 0, coinLoot);
	player.gainExp(expLoot);
}

void attackEnemy(int target, int atk, vector<int>& dead) {
	enemyLoad[target]->takeDamage(atk);
	int netDMG = atk - enemyLoad[target]->getDEF();
	if (netDMG < 0) {
		netDMG = 0;
	}
	if (dungeon.player.getRoomID() != 16 || enemyLoad[target]->getCurHP() != 0) {
		dungeon.player.attackMessage(netDMG, enemyLoad[target]->getName());
	}
	if (enemyLoad[target]->isDead()) {
		if (dungeon.player.getRoomID() != 16 || enemyLoad[target]->getCurHP() != 0) {
			printLine("(It has perished)", 1);
			dungeon.lootDrop(*enemyLoad[target]);
		}
		dead.push_back(target);
	}
	else {
		printLine("(It has " + to_string(enemyLoad[target]->getCurHP()) + " HP left)");
	}
}

void sort(vector<int>& arr) {
	for (int i = 0; i < (int)arr.size(); i++) {
		for (int j = 0; j < (int)arr.size() - i - 1; j++) {
			if (arr[j] < arr[j + 1]) {
				int temp = arr[j];
				arr[j] = arr[j + 1];
				arr[j + 1] = temp;
			}
		}
	}
}

void Dungeon::endingCutscene() { 
	if (endGameID == 1) {
		printLine(script, 35);
		printLine(script, 36);
		printLine(script, 37, 1);
	}
	else if (endGameID == 2) {
		printLine(script, 38);
		printLine(script, 39);
		printLine(script, 40);
		printLine(script, 41, 1);
	}
	else if (endGameID == 3) {
		printLine(script, 42);
		printLine(script, 43);
		printLine(script, 44);
		printLine(script, 45);
		printLine(script, 46, 1);
	}
	printLine("\nEnding " + to_string(endGameID) + "/3 ", 2);
	if (endGameID == 1) {
		printLine("Dead");
	}
	else if (endGameID == 2) {
		printLine("Chicken Out");
	}
	else if (endGameID == 3) {
		printLine("Conquer");
	}
}