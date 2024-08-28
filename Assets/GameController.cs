using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;
using UnityEditor;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameModel model = null;
    public GameObject dripZones;
    public GameObject dripPrefab;
    public GameObject iceCream;
    public GameObject tongue;
    public GameObject brainfreezeMeter;
    public GameObject gameoverDialog;
    public Color brainfreezeColor;
    public Color dripColor;
    private List<GameObject> drips = new List<GameObject>();

    [SerializeField] private GameModel.Settings settings = new GameModel.Settings();

    // Start is called before the first frame update
    void Start()
    {
        gameoverDialog.SetActive(false);

        // // delete all drips
        foreach (GameObject drip in drips)
        {
            Destroy(drip);
        }
        drips.Clear();

        // // get the number of children in the dripZones object
        int numDripZones = dripZones.transform.childCount;

        model = new GameModel(settings, numDripZones);

        // subscribe to the model events
        model.DripEvent += (idx) =>
        {
            CreateDrip(idx);
        };
        model.GameOverEvent += () =>
        {
            Debug.Log("Game Over");
            // show alert dialog that says "Game Over"
            iceCream.SetActive(false);
            // show game over dialog after a 2 second delay, using DOTween
            DOTween.Sequence()
                .AppendInterval(2f)
                .OnComplete(() =>
                {
                    gameoverDialog.SetActive(true);
                });
        };
        Debug.Log("GameController started");
    }

    private void CreateDrip(int idx)
    {
        Assert.IsTrue(idx >= 0 && idx < dripZones.transform.childCount);
        Debug.Log("Drip " + idx + " dripped");

        // spawn drip prefab at this point and add it to the drips list
        drips.Add(Instantiate(dripPrefab, dripZones.transform.GetChild(idx).position, Quaternion.identity));
    }

    bool IsGameActive()
    {
        return iceCream.activeSelf;
    }
    // Update is called once per frame
    void Update()
    {
        if (IsGameActive())
        {
            UpdateBrainfreezeMeter();
            UpdateDrips();
            UpdateDripZones();

            model.Update(Time.deltaTime);
        }

        if (Input.GetMouseButtonDown(0)) // For mouse clicks or touch on the screen
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider == null)
            {
                return;
            }
            else if (gameoverDialog.activeSelf && hit.transform == gameoverDialog.transform)
            {
                // gameoverDialog.SetActive(false);
                // iceCream.SetActive(true);
                // model.Reset();

                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }

            if (!IsGameActive())
            {
                return;
            }
            else if (hit.transform.IsChildOf(dripZones.transform))
            {
                //iterate through all children of the dripZones object
                for (int i = 0; i < dripZones.transform.childCount; i++)
                {
                    if (hit.transform == dripZones.transform.GetChild(i)) // Check if the hit object is the child at index i
                    {
                        model.Lick(i);
                        DoLickAnimation(hit.transform.position);
                        return;
                    }
                }
            }
            else if (hit.transform == iceCream.transform)
            {
                model.LickDripless();
                DoLickAnimation(hit.collider.transform.position);
            }
        }
    }

    private void UpdateDripZones()
    {
        Color clearColor = dripColor;
        clearColor.a = 0;

        for (int i = 0; i < dripZones.transform.childCount; i++)
        {
            Transform dripZone = dripZones.transform.GetChild(i);
            dripZone.GetComponent<SpriteRenderer>().enabled = model.IsDripZoneActive(i);

            float normalizedTime = model.GetDripTimeNormalized(i);
            float inverseNormalizedTime = 1 - normalizedTime;
            dripZone.localScale = new Vector3(Mathf.Lerp(0f, 0.2f, normalizedTime), Mathf.Lerp(0f, 0.2f, normalizedTime), 0);
            dripZone.GetComponent<SpriteRenderer>().color = Color.Lerp(clearColor, dripColor, normalizedTime);
        }
    }

    public float tongueOverShoot = 1f;

    private void DoLickAnimation(Vector3 position)
    {
        //generate a random number between -10 and 10
        float randomX = UnityEngine.Random.Range(-10f, 10f);

        // tween the tongue using DOTween from lower off-screen position to the passed in position. over shoot it a bit so there's a comical lick motion. then tween back to the lower offscreen position
        // tongue.transform.position = new Vector3(position.x + randomX, -10f, position.z);
        DOTween.To(() => tongue.transform.position, x => tongue.transform.position = x, new Vector3(position.x, position.y + tongueOverShoot, position.z), 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            DOTween.To(() => tongue.transform.position, x => tongue.transform.position = x, new Vector3(position.x, -10f, position.z), 0.2f).SetEase(Ease.OutQuad);
        });


        // DOTween.To(() => tongue.transform.position, x => tongue.transform.position = x, position, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
        // {
        //     DOTween.To(() => tongue.transform.position, x => tongue.transform.position = x, iceCream.transform.position, 0.5f).SetEase(Ease.OutBounce);
        // });


        // tongue.transform.position = position;
    }

    private void UpdateBrainfreezeMeter()
    {
        // set rotation such that the min and max rotation are 100 and 260 degrees. a value of 0 is min and kBrainHPFull is max use lerp
        float minRotation = 90f;
        float maxRotation = 240f - 360f;
        float minHP = 0f;
        float maxHP = model.GetBrainHPFull();
        float currentHP = model.GetBrainHP();
        float normalizedHP = 1 - currentHP / maxHP;
        float rotation = Mathf.Lerp(minRotation, maxRotation, normalizedHP);
        brainfreezeMeter.transform.rotation = Quaternion.Euler(0, 0, rotation);
    }
    private void UpdateDrips()
    {
        for (int i = 0; i < drips.Count; i++)
        {
            GameObject drip = drips[i];
            drip.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.clear, dripColor, 1 - drip.transform.position.y / 5);
            if (drip.transform.position.y < -5)
            {
                Debug.Log("Drip removed " + i);
                Destroy(drip);
                drips.RemoveAt(i);
                i--;
            }
        }
    }
}