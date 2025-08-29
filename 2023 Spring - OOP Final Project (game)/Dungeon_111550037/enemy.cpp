#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include "entity.h"
#include "character.h"
#include "item.h"
#include "room.h"
#include "enemy.h"
#include "dialogue.h"

extern vector<Dialogue> enemyDesc;

vector<Enemy> enemySummon() {
	vector<Enemy> list;
	list.push_back(Enemy(0, "Skeleton", 2, 40, 30, 0, 1));
	list.push_back(Enemy(1, "Glizzard", 4, 60, 40, 10, 1));
	list.push_back(Enemy(2, "The Rat King", 5, 50, 30, 20, 1));
	list.push_back(Enemy(3, "Alpine the Aggressor", 7, 80, 60, 35, 2));
	list.push_back(Enemy(4, "Devin the Defender", 7, 80, 35, 60, 2));
	list.push_back(Enemy(5, "Spikey", 11, 55, 70, 45, 2));
	list.push_back(Enemy(6, "Stone Wall", 12, 75, 20, 120, 2));
	list.push_back(Enemy(7, "The Fallen Warrior", 15, 150, 80, 100, 3));
	list.push_back(Enemy(8, "The Lost Warrior", 15, 150, 80, 100, 3));
	list.push_back(Enemy(9, "Krux, the ruler of the undead", 16, 300, 160, 150, 5));
	return list;
}

void Enemy::loadCheck(int _id) {
	extern vector<Enemy*> enemyLoad;
	if (getRoomID() == _id) {
		enemyLoad.push_back(this);
	}
}

void Enemy::showStats() {
	cout << endl << enemyDesc[id].GetLine() << endl << endl;
	cout << "HP: " << getCurHP() << "/" << getMaxHP() << endl;
	cout << "ATK: " << getATK() << endl;
	cout << "DEF: " << getDEF() << endl;
}

void Enemy::attackMessage(int dmg, string target) {
	printLine("(" + getName() + " deals you " + to_string(dmg) + " points)", 1);
}