using System;
using UnityEngine;

namespace Jongmin
{
    public class CardSortSystem : MonoBehaviour
    {
        private CardSortView _view;
        private SortType _sortType;
        private bool _isAscending;
        
        public event Action RequestResetCriterion;

        public void Construct(CardSortView view)
        {
            _view = view;
            _sortType = SortType.Time;
        }

        public void Initialize()
        {
            ResetSort();
            ResetCriterion();
            SyncWithData();
        }

        public void HandleOnClickedLeft()
        {
            SetPrevSortType();
            UpdateSortText();
            SyncWithData();
        }

        public void HandleOnClickedRight()
        {
            SetNextSortType();
            UpdateSortText();
            SyncWithData();
        }

        public void HandleOnClickedCriterion()
        {
            _isAscending = !_isAscending;
            SyncWithData();
        }

        private void ResetCriterion()
        {
            _isAscending = false;
            _view.ResetCriterion();
        }

        private void ResetSort()
        {
            _sortType = SortType.Time;
            UpdateSortText();
        }
        
        private void UpdateSortText()
        {
            var sortText = _sortType switch
            {
                SortType.Time       => "획득순",
                SortType.Grade      => "등급순",
                SortType.Attack     => "공격력순",
                SortType.Defense    => "보호력순",
                _                   => "획득순"
            };        

            _view.SetSortLabel(sortText);
        }
        
        private void SetPrevSortType()
        {
            _sortType = _sortType switch
            {
                SortType.Time       => SortType.Defense,
                SortType.Grade      => SortType.Time,
                SortType.Attack     => SortType.Grade,
                SortType.Defense    => SortType.Attack,
                _                   => SortType.Time
            };
        }

        private void SetNextSortType()
        {
            _sortType = _sortType switch
            {
                SortType.Time       => SortType.Grade,
                SortType.Grade      => SortType.Attack,
                SortType.Attack     => SortType.Defense,
                SortType.Defense    => SortType.Time,
                _                   => SortType.Time
            };
        }

        private void SyncWithData()
        {
            DataCenter.Instance.SortUserCards(_sortType);
            RequestResetCriterion?.Invoke();
        }
    }
}