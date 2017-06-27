using System;
using System.Collections.Generic;

using UIKit;

namespace iFactr.Touch
{
    public static class ModalManager
    {
        public static Queue<ModalTransitionState> ModalQueue { get; private set; }

        public static bool TransitionInProgress { get; private set; }

        static ModalManager()
        {
            ModalQueue = new Queue<ModalTransitionState>();
        }

        /// <summary>
        /// Enqueues a modal presentation or dismissal.
        /// </summary>
        /// <param name='presenter'>
        /// The presenting view controller
        /// </param>
        /// <param name='modal'>
        /// The controller to present or null if dismissing the currently presented controller.
        /// </param>
        /// <param name='animated'>
        /// Whether to animate the presentation or dismissal.
        /// </param>
        public static void EnqueueModalTransition(UIViewController presenter, UIViewController modal, bool animated)
        {
            ModalQueue.Enqueue(new ModalTransitionState(presenter, modal, animated));
            DequeueModalTransition();
        }

        /// <summary>
        /// Returns the uppermost controller in the modal stack i.e. the controller that is not presenting another modal controller.
        /// </summary>
        /// <param name="root">An optional starting point for where to begin the search.
        /// If null, the TouchFactory.Instance.TopViewController will be used.</param>
        public static UIViewController GetTopmostViewController(UIViewController root)
        {
            var controller = root ?? TouchFactory.Instance.TopViewController;
            while (controller.PresentedViewController != null)
            {
                controller = controller.PresentedViewController;
            }
            return controller;
        }

        private static void DequeueModalTransition()
        {
            if (ModalQueue.Count < 1 || TransitionInProgress)
                return;

            var state = ModalQueue.Dequeue();
            if (state.ModalController != null)
            {
                TransitionInProgress = true;

                GetTopmostViewController(state.Presenter).PresentViewController(state.ModalController, state.IsAnimated, () => 
                {
                    TransitionInProgress = false;
                    DequeueModalTransition();
                });
            }
            else if (state.Presenter != null && state.Presenter.PresentedViewController != null)
            {
                TransitionInProgress = true;
                state.Presenter.DismissViewController(state.IsAnimated, () =>
                {
                    TransitionInProgress = false;
                    DequeueModalTransition();
                });
            }
        }
    }

    public class ModalTransitionState
    {
        public UIViewController Presenter { get; set; }
        public UIViewController ModalController { get; set; }
        public bool IsAnimated { get; set; }
        
        public ModalTransitionState(UIViewController presenter, UIViewController modal, bool animated)
        {
            Presenter = presenter;
            ModalController = modal;
            IsAnimated = animated;
        }
    }
}

