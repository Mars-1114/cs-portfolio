#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <string.h>
#include <vector>
#include <conio.h>
#include "dialogue.h"
#include "Dungeon.h"
#include "room.h"

extern vector<List> list;

void generateAction() {
	list.push_back({ 0, "New Game" });
	list.push_back({ 1, "Load Game" });
	list.push_back({ 2, "Move North" });
	list.push_back({ 3, "Move South" });
	list.push_back({ 4, "Move East" });
	list.push_back({ 5, "Move West" });
	list.push_back({ 6, "Move" });
	list.push_back({ 7, "Check Status" });
	list.push_back({ 8, "Attack" });
	list.push_back({ 9, "Retreat" });
	list.push_back({ 10, "Talk" });
	list.push_back({ 11, "Open Chest" });
	list.push_back({ 12, "Open Backpack" });
}

void searchDelete(Enemy* ptr, vector<Enemy>& target) {
	for (int i = 0; i < target.size(); i++) {
		if (target[i].getName() == ptr->getName() && target[i].getRoomID() == ptr->getRoomID()) {
			target.erase(target.begin() + i);
			break;
		}
	}
}

void searchDelete(Item* ptr, vector<Item>& target) {
	for (int i = 0; i < target.size(); i++) {
		if (target[i].getName() == ptr->getName() && target[i].getRoomID() == ptr->getRoomID()) {
			target.erase(target.begin() + i);
			break;
		}
	}
}

void searchDelete(sell* ptr, vector<sell>& target) {
	for (int i = 0; i < target.size(); i++) {
		if (target[i].item.getName() == ptr->item.getName()) {
			target.erase(target.begin() + i);
			break;
		}
	}
}

vector<Dialogue> enemyDesc = {
	{0, "Just a pile of bones"}, //Skeleton
	{1, "A very aggresive lizard, watch out its jaw"}, //Glizzard
	{2, "Sort of ruling this room, although there's no other creatures around"}, //The Rat King
	{3, "He likes to attack with fists. Lost his shield some years ago"}, //Alpine the Aggressor
	{4, "He sold his hammer for another shield. Twice is always better he said"}, //Devin the Defender
	{5, "A giant spike ball. Literally."}, //Spikey
	{6, "He won't let you pass that easily"}, //Stone Wall
	{7, "The guardian of the final room."}, //The Fallen Warrior
	{8, "The guardian of the final room."}, //The Lost Warrior
	{9, "The final boss. The grand finale."} //Krux, the Ruler of The Undead
};