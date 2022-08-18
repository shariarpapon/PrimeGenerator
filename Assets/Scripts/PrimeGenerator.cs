using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class PrimeGenerator : MonoBehaviour
{
    private const string SAVE_DRECTORY = @"C:\Users\Public\Downloads";

    public long lowerBound;
    public long upperBound;

    public long estimatedQuantity;
    public long actualQuantity;
    public long checkingTerm;

    public long numberOfPrimes = 0;

    private bool isChecking = false;
    private float delay = 0.001f;

    public float loadingDelay = 0.15f;
    private bool isLoading = false;
    private int loadPoints = 0;

    private string currentFileName = string.Empty;

    [Header("UI Contents")]
    public TextMeshProUGUI TMPEstimatedQuantity;
    public TextMeshProUGUI TMPActualQuantity;
    public TextMeshProUGUI TMPPrimeList;
    public TextMeshProUGUI TMPDirectory;
    public TMP_InputField TMPDelayInput;
    public TMP_InputField TMPBoundInput;
    public TextMeshProUGUI TMPCheckingTerm;
    public TextMeshProUGUI TMPError;
    public Text loading;

    private void Start()
    {
        TMPDelayInput.text = "0.001";
        delay = GetDelay(TMPDelayInput.text);
    }

    public void StopCalculating()
    {
        StopAllCoroutines();
        isChecking = false;
        isLoading = false;
    }

    public void CheckDelay()
    {
        if (isChecking)
        {
            delay = GetDelay(TMPDelayInput.text);
        }
    }

    public void CheckBounds()
    {
        if (AssignBounds())
            TMPError.gameObject.SetActive(false);
        else
            TMPError.gameObject.SetActive(true);
    }

    public bool AssignBounds()
    {
        string lower = string.Empty;
        string upper = string.Empty;

        char[] chars = TMPBoundInput.text.ToCharArray();

        bool isUpper = false;
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == ',')
                isUpper = true;

            if (isUpper == true && chars[i] != ',')
                upper += chars[i];
            else if (isUpper == false)
                lower += chars[i];
        }
        if (!(long.TryParse(lower, out lowerBound)))
           return false;
        
        if (!(long.TryParse(upper, out upperBound)))
            return false;

        return true;
    }

    private void Update()
    {
        if (isChecking == true)
        {
            if (isLoading == false)
            {
                StartCoroutine(Load());
            }
        }
        else
        {
            if (loading.text != string.Empty)
                loading.text = string.Empty;
        }
    }

    public IEnumerator Load()
    {
        isLoading = true;
        if (loadPoints >= 10)
        {
            loading.text = string.Empty;
            loadPoints = 0;
        }
        else
        {
            loading.text += ".";
            loadPoints++;
        }
        yield return new WaitForSeconds(loadingDelay);
        isLoading = false;
    }

    public void Calculate()
    {
        Initialize();

        if (upperBound >= lowerBound && upperBound > 0 && lowerBound > 0)
        {
            //Special Case
            if (lowerBound <= 2 && upperBound >= 2)
            {
                TMPPrimeList.text += "2 ,";
                numberOfPrimes++;
            }

            bool isEven = NumberIsEven(lowerBound);

            if (isChecking == false)
            {
                UpdateAllUI();
                StartCoroutine(PrimeCheck(isEven));
            }
        }
    }

    public IEnumerator PrimeCheck(bool isEven)
    {
        isChecking = true;
        for (long i = 0; i <= upperBound; i++)
        {
            long currentNumber = lowerBound + GetIncrement(i, isEven);

            if (currentNumber > upperBound)
                break;

            if (IsPrime(currentNumber))
            {
                TMPPrimeList.text += currentNumber.ToString() + ", ";
                numberOfPrimes++;
                actualQuantity = numberOfPrimes;
            }
            checkingTerm++;
            UpdateAllUI();
            yield return new WaitForSeconds(delay);
        }
        isChecking = false;
    }

    private float GetDelay(string input)
    {
        float delay;
        float.TryParse(input, out delay);
        return delay;
    }

    private bool IsPrime(long num)
    {
        if (num == 1)
            return false;

        long possibleFactors = (long)num / 2;

        for (long i = 0; i <= possibleFactors; i = i * 2 +1)
        {
          
            for (long j = 0; j <= possibleFactors; j = j + 1)
            {
                long product = i * j;

                if (product == num)
                    return false;
            }
        }
        return true;
    }

    private void Initialize()
    {
            if (TMPError.gameObject.activeSelf)
                TMPError.gameObject.SetActive(false);

            numberOfPrimes = 0;
            actualQuantity = 0;
            loadPoints = 0;
            checkingTerm = 0;
            loading.text = string.Empty;
            isChecking = false;
            delay = GetDelay(TMPDelayInput.text);
            estimatedQuantity = GetEstimatedQuantity();
            TMPEstimatedQuantity.text = string.Empty;
            TMPActualQuantity.text = string.Empty;
            TMPPrimeList.text = string.Empty;
            TMPPrimeList.pageToDisplay = 1;
            UpdateAllUI();
    }

    private long GetIncrement(long index, bool isEven)
    {
        if (isEven)
            return ((index * 2) + 1);
        else
            return (index * 2);
    }

    private bool NumberIsEven(long number)
    {
        if (number == 1)
            return false;

        float a = (float)number / 2;
        float b = Mathf.Ceil(a);

        if (a == b)
            return true;
        else
            return false;
    }

    private long GetEstimatedQuantity()
    {
        long output = 0;
        if (lowerBound > 1)
        {
            output = (long)((upperBound / (float)Mathf.Log(upperBound)) - (lowerBound / (float)Mathf.Log(lowerBound)));
        }
        else if (lowerBound == 1)
        {
            output = upperBound / (long)Mathf.Log(upperBound);
        }
        return output;
    }

    private void UpdateAllUI()
    {
        UpdateQuantitesUI();
        TMPCheckingTerm.text = "Checking Term : " + checkingTerm.ToString();
    }

    private void UpdateQuantitesUI()
    {
        TMPEstimatedQuantity.text = "Estimated Quantity : " + estimatedQuantity.ToString();
        TMPActualQuantity.text = "Actual Quantity : " + actualQuantity.ToString();
    }

    public void SaveList(TMP_InputField fileName)
    {
        if (TMPPrimeList.text != string.Empty)
        {
            if (fileName.text == string.Empty || fileName.text == currentFileName)
            {
                System.Random prng = new System.Random( ( Time.frameCount.ToString() + System.DateTime.Now.ToString() ).GetHashCode() );
                fileName.text = "prime-list-" + System.DateTime.Now.ToString() + prng.Next((int)Random.Range(0, 99999999)).ToString() + ".txt";
                currentFileName = fileName.text;
            }
            else
                fileName.text += ".txt";

            string content = TMPPrimeList.text;

            string path = SAVE_DRECTORY;

            if (Application.platform != RuntimePlatform.OSXPlayer)
            {
                path += "\\" + fileName.text;
            }
            else
                path = Application.persistentDataPath + "\\" + fileName.text;

            File.WriteAllText(path, content);

            TMPDirectory.text = path;
        }
    }

    public void GoNext()
    {
        if(TMPPrimeList.textInfo.pageCount > 1 && TMPPrimeList.pageToDisplay < TMPPrimeList.textInfo.pageCount)
            TMPPrimeList.pageToDisplay++;

    }

    public void GoPrevious()
    {
        if (TMPPrimeList.pageToDisplay > 1)
            TMPPrimeList.pageToDisplay--;
    }

    public void QuitProgram()
    {
        Application.Quit();
    }

}