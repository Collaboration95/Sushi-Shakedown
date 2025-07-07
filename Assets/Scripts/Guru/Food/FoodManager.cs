using UnityEditor;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    //3 arrays, rice fish condiement for food orders
    [SerializeField] private DraggableObjectSO[] riceDraggableObjectSOArray;
    [SerializeField] private DraggableObjectSO[] fishDraggableObjectSOArray;
    [SerializeField] private DraggableObjectSO[] condimentDraggableObjectSOArray;

    //drinkdraggableobjectsoarray for drink orders
    [SerializeField] private DraggableObjectSO[] drinkDraggableObjectSOArray;

    [SerializeField] private GameObject platePrefab;
    [SerializeField] private GameObject cupPrefab;

    public GameObject foodPrefab;
    Sprite[] sprites;

    void Start()
    {
        sprites = Resources.LoadAll<Sprite>("Food");
    }

    public Food GetRandomFood()
    {

        //decide if food or drinik
        int randomFoodOrDrink = Random.Range(0, 2); // 0 for food, 1 for drink


        //decide if food or drink
        if (randomFoodOrDrink == 0) //if food
        {
            //food
            //choose a random riceDraggableObjectSO from the array
            int randomRiceIndex = Random.Range(0, riceDraggableObjectSOArray.Length);
            DraggableObjectSO randomRice = riceDraggableObjectSOArray[randomRiceIndex];
            //choose a random fishDraggableObjectSO from the array
            int randomFishIndex = Random.Range(0, fishDraggableObjectSOArray.Length);
            DraggableObjectSO randomFish = fishDraggableObjectSOArray[randomFishIndex];
            //choose a random condimentDraggableObjectSO from the array
            int randomCondimentIndex = Random.Range(0, condimentDraggableObjectSOArray.Length);
            DraggableObjectSO randomCondiment = condimentDraggableObjectSOArray[randomCondimentIndex];

            //Instantiate plate prefab
            GameObject plate = Instantiate(platePrefab, Vector3.zero, Quaternion.identity);

            //Set the plate's fish, rice, and condiment sprite
            plate.GetComponent<PlateDraggable>().SetRiceSprite(randomRice.sprite);
            plate.GetComponent<PlateDraggable>().SetFishSprite(randomFish.sprite);
            plate.GetComponent<PlateDraggable>().SetCondimentSprite(randomCondiment.sprite);


            //disable box collider, treat this gameobject as a sprite plate
            plate.GetComponent<BoxCollider2D>().enabled = false;

            Food foodComponent = plate.GetComponent<Food>();
            foodComponent.ingredientsDraggableObjectSOArray.Add(randomRice);
            foodComponent.ingredientsDraggableObjectSOArray.Add(randomFish);
            foodComponent.ingredientsDraggableObjectSOArray.Add(randomCondiment);
            return foodComponent;
        }
        else
        {
            //Instantiate cup prefab
            GameObject cup = Instantiate(cupPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log(cup.GetComponent<CupDraggable>().drinkSpriteArray.Count);
            Debug.Log(cup.GetComponent<CupDraggable>().drinkSpriteArray);

            //disable box collider, treat this gameobject as a sprite plate
            cup.GetComponent<BoxCollider2D>().enabled = false;
            Food foodComponent = cup.GetComponent<Food>();
            //choose a random int from 1 to 3 to decide how many drinks to add (from 1 to 3)
            int randomDrinkCount = Random.Range(1, 4); // 1 to 3 drinks
            for (int i = 0; i < randomDrinkCount; i++)
            {
                //choose a random drinkDraggableObjectSO from the array
                int randomDrinkIndex = Random.Range(0, drinkDraggableObjectSOArray.Length);
                DraggableObjectSO randomDrink = drinkDraggableObjectSOArray[randomDrinkIndex];

                //Set the cup's drink sprite
                cup.GetComponent<CupDraggable>().SetDrinkSprite(randomDrink.sprite, i);

                //add drink ingredient into food component
                foodComponent.ingredientsDraggableObjectSOArray.Add(randomDrink);
            }
            return foodComponent;
        }


        // int randomIndex = Random.Range(0, sprites.Length);
        // Sprite randomSprite = sprites[randomIndex];

        // GameObject foodGO = Instantiate(foodPrefab, Vector3.zero, Quaternion.identity);
        // Food foodComponent = foodGO.GetComponent<Food>();

        // foodComponent.spriteRenderer.sprite = randomSprite;
        // foodComponent.foodName = randomSprite.name;

        // return foodComponent;
    }
}
