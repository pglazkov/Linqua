using System;
using System.Collections.Generic;
using System.Text;
using Framework;

namespace Linqua.Events
{
	public class StopFirstUseTutorialEvent : EventBase
    {
	    public StopFirstUseTutorialEvent(FirstUseTutorialType tutorialType)
	    {
	        TutorialType = tutorialType;
	    }

	    public FirstUseTutorialType TutorialType { get; private set; }
    }
}
