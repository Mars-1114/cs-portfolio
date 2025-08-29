#include <vector>
#include <conio.h>
#include "dialogue.h"

vector<Dialogue> addScript() {
	vector<Dialogue> temp = {
		//intro
		{ 0, "(Oh, a challenger...)"},
		{ 1, "(It's been ages, but still, welcome.)"},
		{ 2, "(I'm the gatekeeper of this dungeon.)"},
		{ 3, "(So, I assume you already know how this works.)"},
		{ 4, "(Just a few more things to do and you're good to go.)"},
		{ 5, "(Sign your name here first, or whatever you like us to call you.)"},
		{ 6, "(And select one occupation you prefer. Choose wisely as this is NOT invertible.)"},
		{ 7, "(Well, it's done, now have fun.)"},
		//if the room has enemy(s)
		{ 8, "(You stepped into a dark room, having a bad feeling)"},
		{ 9, "(You came across a large hallway, creatures stand before your sight)"},
		{10, "(You carefully opened the rusted door, you heard something unpleasant.)"},
		{11, "(You peeked through the entrance, finding a hostile creature)"},
		{12, "(You pulled a heavy lever, hearing a terrifying stomp)"},
		//+ but if the player already went there
		{13, "(You take back your courage and fight once again)"},
		{14, "(After fully prepared, you returned to the battle)"},
		//if retreat
		{15, "(You decided that you are not ready, and backed out)"},
		//if at room 2
		{16, "(The door seems to be locked from the other side)"},
		//if at room 9
		{17, "Smith: that door? yeah i locked it to prevent, y'know, theft."},
		{18, "Smith: and i locked the key in a toolbox to prevent, y'know, theft."},
		//+ but if already went there
		{19, "Smith: bring me the toolbox back, and i'll give you the key."},
		//if at room 6
		{20, "(This room is oddly cozy, even has some food smell.)"},
		{21, "(You suddenly realized you haven't eaten for a while.)"},
		//if at room 15
		{22, "(This door is locked. But there's a coin slot attached to it.)"},
		{23, "(It reads: \"50G per challenger to pass.\")"},
		{24, "(You're confused about this sussy mechanic.)"},
		//+ this only when already went there
		{25, "(Spend 50G unlocking this door?)"},
		//+ if unlock
		{26, "(It turns out it just leads to the main entrance ...at least you don't need to walk for another 30 minutes.)"},
		//if attempt to enter room 16
		{27, "(As you opened this heavy gate, you felt something isn't right.)"},
		{28, "(You might not be able to leave this room.)"},
		{29, "(This is your last chance to prepare.)"},
		//enter room 16
		{30, "(You entered the room, and the door immediately shut close.)"},
		{31, "(What's inside is a large chamber, with pillars and a hanging chandelier.)"},
		{32, "(This should be the moment when the dramatic music plays. Sadly this game runs on command prompt.)"},
		{33, "(Anyway the boss descended from above, Krux is his name.)"},
		{34, "(Prepare for the battle.)"},
		//cutscene "dead"
		{35, "(Even though you want to keep moving, your body can't take it anymore.)"},
		{36, "(Perhaps there has more improvements you can do, perhaps it's not the time yet.)"},
		{37, "(You will do it better.)"},
		//cutscene "chicken out"
		{38, "(You noticed the entrance is wide open and not even hesitated.)"},
		{39, "(This, despite being a galaxy brain move, is not how you supposed to play.)"},
		{40, "(However, to award this kind of bravery(of challenging the game design), I must give you some credit.)"},
		{41, "(Have a nice day.)"},
		//cutscene "conquer"
		{42, "(Krux fell onto the ground, struggling to stand up.)"},
		{43, "(He then teleported out as you tried to give him one last hit.)"},
		{44, "(I guess you can count this as a win.)"},
		{45, "(You have finally conquered this dungeon.)"},
		{46, "(Thanks for playing)"},
		//unlock room 10
		{47, "Smith: oh you actually found it ...appreciate that."},
		{48, "Smith: here's the key you're looking for. thanks again."},
		//
		{49, "(You heard something unlocked from a distance.)"},
		{50, "(...You don't have it.)"},
		{51, "Smith: and i lost the toolbox."},
		{52, "Smith: brilliant me."},
		{53, "Smith: if you find it, i'll give you what's inside."},
		//give Smith the toolbox without knowing it's his
		{54, "Smith: ...how do you know it's mine?"},
		{55, "Smith: anyway here's the key."}
	};
	return temp;
}

void printLine(vector<Dialogue> pool, int id, int mode)
{
	char input = ' ';
	string s = pool[id].GetLine();
	cout << s;
	if (mode != 2) {
		cout << endl;
	}
	if (mode == 0) {
		while (input != 'c' && input != 'C') {
			input = _getch();
		}
		system("cls");
	}
}

void printLine(string s, int mode) {
	char input = ' ';
	cout << s;
	if (mode != 2) {
		cout << endl;
	}
	if (mode == 0) {
		while (input != 'c' && input != 'C') {
			input = _getch();
		}
		system("cls");
	}
}