using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using System.Linq;
using UniRx.Triggers;
using UnityEngine.UI;

public class UniRxSample : MonoBehaviour
{
    [SerializeField]
    Button button = null;

    /// <summary>
    /// UniRx Subject。
    /// </summary>
    public Subject<string> subject = new Subject<string>();

    void Awake()
    {
        // UniRx Subject登録。
        subject
        .Subscribe(
            message => {
                Debug.Log(message);
            },
            () => {
                Debug.Log("Completed");
            }
        ).AddTo(gameObject);
    }

    void Start()
    {
        // UniRx Subject呼び出し。
        subject.OnNext("UniRx Hello!");
        subject.Dispose();
        subject.OnNext("UniRx Hello!");
        subject.OnCompleted();

        var reactiveProperty = new ReactiveProperty<string>("sample");
        reactiveProperty.Value = "Hello";
        reactiveProperty.Subscribe(e => Debug.Log(e));
        reactiveProperty.Value = "hello";

        var reactiveCollection = new ReactiveCollection<int>();
        reactiveCollection
            .ObserveAdd()
            .Subscribe(x => {
                Debug.Log("Add Index: " + x.Index + " Value: " + x.Value);
            });
        
        reactiveCollection
            .ObserveRemove()
            .Subscribe(x => {
                Debug.Log("Remove Index:" + x.Index + " Value: " + x.Value);
            });

        reactiveCollection.Add(1);
        reactiveCollection.Add(2);
        reactiveCollection.Remove(2);

        Observable.Create<int>(ob => {
            ob.OnNext(1);
            ob.OnCompleted();
            return Disposable.Create(() => {});
        }).Subscribe(e => Debug.Log(e));
        Debug.Log("mainThread");

        Observable.Start(() => {
            var count = 0;
            for(var idx = 0; idx < 10000; ++idx)
            {
                count += idx;
            }
            return count;   
        })
        .ObserveOnMainThread()
        .Subscribe(e => Debug.Log(e));
        Debug.Log("mainThread");

        Observable.Timer(System.TimeSpan.FromSeconds(3))
        .Subscribe(e => Debug.Log("elapsed 3sec"));

        Observable.Timer(System.TimeSpan.FromSeconds(3), System.TimeSpan.FromSeconds(1))
        .Subscribe(e => Debug.Log("elapsed 3sec and repeat 1sec"))
        .AddTo(gameObject);

        this.FixedUpdateAsObservable()
            .Subscribe(e => Debug.Log("fixed update"));

        button.OnClickAsObservable().Subscribe(e => Debug.Log("onclick"));

        Observable.EveryUpdate().Subscribe(e => Debug.Log("every update"));

        Observable.FromCoroutine(TestCoroutine)
            .Subscribe(
                e => {
                    Debug.Log("next");
                },
                () => {
                    Debug.Log("complete");
                }    
            ).AddTo(gameObject);

        Observable.FromCoroutineValue<int>(ReturnValueCoroutine)
                    .Subscribe(e => Debug.Log(e));

        Observable.FromCoroutine<int>(observer => CallOnNextCoroutine(observer))
                .Subscribe(
                    e => {
                        Debug.Log(e);
                    }
                ).AddTo(gameObject);
    }

    IEnumerator TestCoroutine()
    {
        Debug.Log("start");
        yield return new WaitForSeconds(4);
        Debug.Log("finish");
    }

    IEnumerator ReturnValueCoroutine()
    {
        for(var idx = 0; idx < 10; ++idx)
        {
            yield return idx;
        }
    }

    IEnumerator CallOnNextCoroutine(IObserver<int> observer)
    {
        observer.OnNext(1);
        yield break;
    }
}
