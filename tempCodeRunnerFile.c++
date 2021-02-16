#include <iostream>
#include <limits.h>
#include <string.h>

using namespace std;
//2 5 + => 2+5
// 5 2 / => 5/2 = 2
#define MAX_PAMAT 40

enum stav_zasobnika{OK=-1, FULL=INT_MAX, EMPTY=INT_MIN};

typedef struct {
    int pamat[MAX_PAMAT];
    int vrchol;
} LIFO;

/**
 * @brief Vlozi do zasobnika z hodnotu v
 * @param z Zasobnik, do ktoreho vkladame hodnotu
 * @param v vkladana hodnota 
 * @return v pripade uspechu vrati OK, inak FULL
 */


stav_zasobnika push(LIFO &z, int v){
    cout<<"PUSH"<<v<<endl;
    if(z.vrchol < MAX_PAMAT){
        z.pamat[z.vrchol++] = v;
    }else{
        return FULL;
    }
    return OK;
}

void show(LIFO &zasobnik){
    cout<<"|";
    for(int i=0; i<zasobnik.vrchol; i++) {
        cout<<zasobnik.pamat[i]<<"|";
    }
    cout<<endl;
}

int pop(LIFO &zasobnik) {
    if(zasobnik.vrchol>0)
        return zasobnik.pamat[--zasobnik.vrchol];
    else
        return EMPTY;
}

int main()
{
    LIFO zasobnik;
    zasobnik.vrchol = 0; // inicializacia prazdneho zasobnika
    /* overenie funkcnosti zasobnika
    push(zasobnik, 5);
    push(zasobnik, 7);
    push(zasobnik, 1);
    show(zasobnik);
    pop(zasobnik);
    show(zasobnik)
    */
    char vyraz[40], buffer[20];
    int N; //dlzka vstupneho retazca - vyraz
    cin.getline(vyraz,39);      // 28 5 - 2 *
    N = strlen(vyraz);
    int j;
    int x;
    int a,b;
    for (int i=0 ; i<N ; i++){

        if(vyraz[i] == ' '){    //medzera je oddelovac, ideme na dalsiu iteraciu
            continue;
        }

        if(vyraz[i] == '+'){
            a = pop(zasobnik);
            b = pop(zasobnik);
            push(zasobnik, (a+b));
            continue;
        }

        if(vyraz[i] == '*'){
            a = pop(zasobnik);
            b = pop(zasobnik);
            push(zasobnik, (a*b));
            continue;
        }

        if(vyraz[i] == '/'){
            a = pop(zasobnik);
            b = pop(zasobnik);
            push(zasobnik, (b/a));
            continue;
        }

        if(vyraz[i] == '-'){
            a = pop(zasobnik);
            b = pop(zasobnik);
            push(zasobnik, (b-a));
            continue;
        }

        j=0;
        //parsovanie ciselnych hodnot
        while (vyraz[i]>='0' && vyraz[i]<='9'){
            buffer[j] = vyraz[i];       // [ '2' , '8' ,...], j=2
            i++;
            j++;
        }
        buffer[j] = 0;  // "28"
        x = atoi(buffer);
        // ulozim nacitane cislo do zasobnika
        push(zasobnik, x);
        show(zasobnik);
    }
    cout<<pop(zasobnik);
    
    return 0;
    
    
} 

