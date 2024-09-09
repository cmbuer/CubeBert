/* Copyright (C) 2024 Christopher Buer */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VerticalHold : MonoBehaviour
{
    public GameObject blurredFrame1;
    public GameObject blurredFrame2;
    public GameObject sharpFrame;
    public GameObject menuFrame;
    public GameObject staticObject;
    public VideoPlayer staticVideo;

    private RectTransform blurredXform1;
    private RectTransform blurredXform2;
    private RectTransform sharpXform;
    private RectTransform menuXform;

    private RawImage blurredImage1;
    private RawImage blurredImage2;
    private RawImage sharpImage;
    private RawImage menuImage;
    private RawImage staticImage;

    private Vector3 framePosition1;
    private Vector3 framePosition2;

    private void Start()
    {
        blurredXform1 = blurredFrame1.GetComponent<RectTransform>();
        blurredXform2 = blurredFrame2.GetComponent<RectTransform>();
        sharpXform = sharpFrame.GetComponent<RectTransform>();
        menuXform = menuFrame.GetComponent<RectTransform>();
        blurredImage1 = blurredFrame1.GetComponent<RawImage>();
        blurredImage2 = blurredFrame2.GetComponent<RawImage>();
        sharpImage = sharpFrame.GetComponent<RawImage>();
        menuImage = menuFrame.GetComponent<RawImage>();
        staticImage = staticObject.GetComponent<RawImage>();

        framePosition1 = blurredXform1.position;
        framePosition2 = blurredXform2.position;

        StartCoroutine("PlayShow");
    }

    private IEnumerator PlayShow()
    {
        const float staticInitFadeOutTime = 5.0f;
        const float staticSecondFadeTime = 0.25f;
        const float sharpenTime = 0.5f;
        const float sharpenWaitTime = 2.0f;
        const float bufferWidth = 50;
        const int upRotations = 6;
        const float rotationSpeedDecrement = 200;

        staticVideo.Play();
        Hide(sharpImage);
        Hide(menuImage);
        ShowOpaque(blurredImage1);
        ShowOpaque(blurredImage2);
        ShowOpaque(staticImage);
        FadeOut(staticImage, staticInitFadeOutTime);

        int currentRotation = 0;
        float rotationSpeed = 2000;

        while (currentRotation < upRotations)
        {
            if (blurredXform2.position.y < framePosition1.y)
            {
                blurredXform1.Translate(Vector3.up * Time.deltaTime * rotationSpeed);
                blurredXform2.Translate(Vector3.up * Time.deltaTime * rotationSpeed);
            }
            else
            {
                blurredXform1.position = framePosition1;
                blurredXform2.position = framePosition2;
                rotationSpeed -= rotationSpeedDecrement;
                currentRotation++;
            }
            yield return null;
        }

        staticVideo.Stop();
        Hide(staticImage);

        currentRotation = 0;
        while (currentRotation < 3)
        {
            if (currentRotation == 0)
            {
                blurredXform1.position = blurredXform1.position + new Vector3(0, blurredXform1.rect.height, 0);
                blurredXform2.position = blurredXform2.position + new Vector3(0, blurredXform2.rect.height, 0);
                currentRotation++;
            }
            else if (currentRotation == 1)
            {
                if (blurredXform2.position.y > framePosition2.y - bufferWidth) // down
                {
                    blurredXform1.Translate(Vector3.down * Time.deltaTime * rotationSpeed);
                    blurredXform2.Translate(Vector3.down * Time.deltaTime * rotationSpeed);
                }
                else
                {
                    rotationSpeed -= rotationSpeedDecrement;
                    currentRotation++;
                }
            }
            else if (currentRotation == 2)  // up
            {
                if (blurredXform1.position.y < framePosition1.y)
                {
                    blurredXform1.Translate(Vector3.up * Time.deltaTime * rotationSpeed);
                    blurredXform2.Translate(Vector3.up * Time.deltaTime * rotationSpeed);
                }
                else
                {
                    currentRotation++;
                }
            }
            yield return null;
        }

        blurredXform1.position = framePosition1;
        sharpXform.position = framePosition1;
        Hide(blurredImage2);
        ShowOpaque(sharpImage);
        FadeOut(blurredImage1, sharpenTime);
        yield return new WaitForSeconds(sharpenWaitTime);

        Hide(blurredImage1);
        staticVideo.Play();
        staticVideo.SetDirectAudioMute(0, true);
        FadeIn(staticImage, staticSecondFadeTime);
        yield return new WaitForSeconds(staticSecondFadeTime);

        Hide(sharpImage);
        ShowOpaque(menuImage);
        FadeOut(staticImage, staticSecondFadeTime);
        yield return new WaitForSeconds(staticSecondFadeTime);

        staticVideo.Stop();
        Hide(staticImage);
        menuXform.position = framePosition1;
        SceneManager.LoadScene("MainMenu");
        yield break;
    }

    private void ShowOpaque(RawImage image)
    {
        Color color = image.color;
        color.a = 1;
        image.color = color;
        image.enabled = true;
    }

    private void Hide(RawImage image)
    {
        image.enabled = false;
    }

    private void FadeIn(RawImage image, float time)
    {
        /* the bizarre logic here is workaround for a known bug in CrossFadeAlpha
         * see https://stackoverflow.com/questions/42330509/crossfadealpha-not-working) */
        Color color = image.color;
        color.a = 1;
        image.color = color;
        image.enabled = true;
        image.CrossFadeAlpha(0, 0, false);
        image.CrossFadeAlpha(1, time, false);
    }

    private void FadeOut(RawImage image, float time)
    {
        Color color = image.color;
        color.a = 1;
        image.color = color;
        image.CrossFadeAlpha(0, time, false);
    }
}
