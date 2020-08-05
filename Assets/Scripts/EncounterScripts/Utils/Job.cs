using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Job : ThreadedJob
{
    private readonly Action job;
    public Job(Action job)
    {
        this.job = job;
    }
    protected override void ThreadFunction()
    {
        job();
    }
    protected override void OnFinished()
    {
        //Debug.Log("Finished with job");
    }
}