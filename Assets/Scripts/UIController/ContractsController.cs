using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using Contracts;

// TODO: Make usage of IContract vs IContract.Id more uniform

public class ContractsController : MonoBehaviour
{
    [Header("Available Contracts List")]
    public RectTransform ContractsList;
    public RectTransform ListElementPrefab;
    [Tooltip("Color for contract in list that is selected")]
    public Color ActiveContractColor;

    [Header("Contract Detail View")]
    public RectTransform DetailView;
    public RectTransform Content;
    public RectTransform TodoItem;

    public IContract[] WaitingContracts
    {
        get
        {
            IContract[] contracts = new IContract[_waitingContracts.Count];
            _waitingContracts.Values.CopyTo(contracts, 0);
            return contracts;
        }
    }

    public IContract[] RunningContracts
    {
        get
        {
            IContract[] contracts = new IContract[_runningContracts.Count];
            _runningContracts.Values.CopyTo(contracts, 0);
            return contracts;
        }
    }

    private Dictionary<Guid, IContract> _waitingContracts = new Dictionary<Guid, IContract>();
    private Dictionary<Guid, IContract> _runningContracts = new Dictionary<Guid, IContract>();
    private List<Guid> _fulfillableContracts = new List<Guid>();
    private IContract _contract = null;
    private RectTransform[] _todoItems;
    private int _hackCounter;

    private Transform _startButton, _abortButton, _fulfillButton, _buttons;
    private TextMeshProUGUI _detailsTitle, _detailsDescription;

    private Color _defaultElementColor;
    private Coroutine _checkCoroutine;

    private void Awake()
    {
        _buttons = DetailView.Find("Buttons");
        _startButton = _buttons.Find("Start Button");
        _abortButton = _buttons.Find("Abort Button");
        _fulfillButton = _buttons.Find("Fulfill Button");
        _detailsTitle = DetailView.Find("Title").GetComponent<TextMeshProUGUI>();
        _detailsDescription = Content.Find("Description").GetComponent<TextMeshProUGUI>();
        _defaultElementColor = ListElementPrefab.GetComponent<Image>().color;
    }

    private void OnEnable()
    {
        if (_contract == null)
        {
            DetailView.gameObject.SetActive(false);
        }
        else
        {
            DetailView.gameObject.SetActive(true);
            OpenContract(_contract.Id);
        }
    }

    private class ContractsUpdater : MonoBehaviour { }
    private void Start()
    {
        GameObject contractsUpdater = new GameObject("ContractsUpdater");
        _checkCoroutine = contractsUpdater.AddComponent<ContractsUpdater>().StartCoroutine(CheckContractsStatus());
    }

    private IEnumerator CheckContractsStatus()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(1);
            foreach (var pair in _runningContracts)
            {
                if (!_fulfillableContracts.Contains(pair.Key) && pair.Value.CanFulfill())
                {
                    _fulfillableContracts.Add(pair.Key);
                    FindObjectOfType<NotificationsController>(true).NewNotification("Contract can be fulfilled",
                        "A contract can be fulfilled: " + pair.Value.Title,
                        onClick: () =>
                        {
                            FindObjectOfType<UIController>(true).SetActiveScreen(UIController.Screen.Contracts);
                            OpenContract(pair.Key);
                        },
                        onUpdate: _ =>
                        {
                            if (pair.Value.Fulfilled)
                                FindObjectOfType<NotificationsController>(true).RemoveNotification(_);
                        });
                }
                    
            }
        }
    }

    private void AddContractToList(IContract contract)
    {
        RectTransform element = Instantiate(ListElementPrefab, ContractsList);
        element.name = contract.Id.ToString();
        element.GetComponentInChildren<TextMeshProUGUI>().text = contract.Title;
        if (_runningContracts.ContainsKey(contract.Id))
        {
            element.SetSiblingIndex(ContractsList.Find("Running").GetSiblingIndex() + 1);
        }
        else
        {
            element.SetSiblingIndex(ContractsList.Find("Available").GetSiblingIndex() + 1);
        }
        element.GetComponent<Button>().onClick.AddListener(() => SelectContractFromList(element));
    }

    private void RemoveContractFromList(Guid id)
    {
        Destroy(ContractsList.Find(id.ToString()).gameObject);
    }

    /// <summary>
    /// Add a new contract to the list of waiting contracts and make it available to the player to start the contract.
    /// </summary>
    /// <param name="contract">The IContract instance to add.</param>
    public IContract NewContract(IContract contract)
    {
        if (contract.Id == Guid.Empty)
            contract.Id = Guid.NewGuid();
        _waitingContracts.Add(contract.Id, contract);
        AddContractToList(contract);
        FindObjectOfType<NotificationsController>(true).NewNotification("New Contract",
            "A new contract is available: " + contract.Title,
            onClick: () =>
            {
                _contract = contract;
                FindObjectOfType<UIController>().SetActiveScreen(UIController.Screen.Contracts);
            },
            onUpdate: _ =>
            {
                if (contract.Running)
                    FindObjectOfType<NotificationsController>(true).RemoveNotification(_);
            });
        return contract;
    }

    public void SelectContractFromList(RectTransform element)
    {
        Guid contractId = Guid.Parse(element.name);
        foreach (RectTransform child in ContractsList)
        {
            Image image;
            if (child.TryGetComponent(out image))
                image.color = _defaultElementColor;
        }
        element.GetComponent<Image>().color = ActiveContractColor;

        OpenContract(contractId);
    }

    public void OpenContract(Guid contractId)
    {
        bool waiting = _waitingContracts.ContainsKey(contractId);
        if (!waiting && !_runningContracts.ContainsKey(contractId))
        {
            Debug.LogError("Contract to open not in list: " + contractId);
            return;
        }

        // Remove old todo items
        if (_todoItems != null)
            foreach (RectTransform item in _todoItems)
                Destroy(item.gameObject);
        _todoItems = null;

        DetailView.gameObject.SetActive(true);
        IContract contract = null;
        if (waiting)
        {
            // Contract not started yet
            _fulfillButton.gameObject.SetActive(false);
            _abortButton.gameObject.SetActive(false);
            _startButton.gameObject.SetActive(true);
            contract = _waitingContracts[contractId];
        }
        else
        {
            // Contract is running
            _fulfillButton.gameObject.SetActive(true);
            _abortButton.gameObject.SetActive(true);
            _startButton.gameObject.SetActive(false);
            contract = _runningContracts[contractId];

            _todoItems = new RectTransform[contract.TodoItems.Length];
            for (int i = 0; i < contract.TodoItems.Length; i++)
            {
                _todoItems[i] = Instantiate(TodoItem, Content);
                _todoItems[i].GetComponentInChildren<TextMeshProUGUI>().text = contract.TodoItems[i].Text;
                _todoItems[i].Find("Checkbox").Find("Check").gameObject.SetActive(contract.TodoItems[i].IsFulfilled());
            }

            if (contract.CanFulfill())
                _fulfillButton.GetComponent<Button>().interactable = true;
            else
                _fulfillButton.GetComponent<Button>().interactable = false;
        }

        _detailsTitle.text = contract.Title;
        _detailsDescription.text = contract.Text;
        _detailsDescription.ForceMeshUpdate();
        float height = _detailsDescription.GetRenderedValues().y;

        Vector2 size = _detailsDescription.rectTransform.sizeDelta;
        size.y = height;
        _detailsDescription.rectTransform.sizeDelta = size;
        _contract = contract;
        _hackCounter = 0;
    }

    public void StartContract(IContract contract)
    {
        try
        {
            _waitingContracts.Remove(contract.Id);
        }
        catch
        {
            return;
        }
        _runningContracts.Add(contract.Id, contract);
        RemoveContractFromList(contract.Id);
        AddContractToList(contract);
        OpenContract(contract.Id);
        contract.Running = true;
        contract.OnStart();
    }

    public void StartSelectedContract()
    {
        StartContract(_contract);
        OpenContract(_contract.Id);
    }

    public void FulfillContract(IContract contract)
    {
        if (!_contract.CanFulfill())
            return;
        try
        {
            _runningContracts.Remove(contract.Id);
        }
        catch
        {
            return;
        }
        RemoveContractFromList(contract.Id);
        contract.Running = false;
        contract.Fulfilled = true;
        contract.OnFulfill();
    }


    public void FulfillSelectedContract()
    {
        FulfillContract(_contract);
        _contract = null;
        DetailView.gameObject.SetActive(false);
    }

    public void AbortContract(IContract contract)
    {
        try
        {
            _runningContracts.Remove(contract.Id);
        }
        catch
        {
            return;
        }
        RemoveContractFromList(contract.Id);
        contract.Running = false;
        contract.OnAbort();
    }

    public void AbortSelectedContract()
    {
        AbortContract(_contract);
        _contract = null;
        DetailView.gameObject.SetActive(false);
    }
    public void Hack()
    {
        if (++_hackCounter >= 4)
            if (_contract != null && _contract.Running)
            {
                _contract.Hack();
                FulfillSelectedContract();
            }
    }

    public void LoadContracts(IContract[] available, IContract[] running)
    {
        foreach (IContract contract in available)
        {
            if (contract.Id == Guid.Empty)
                contract.Id = Guid.NewGuid();
            _waitingContracts.Add(contract.Id, contract);
            AddContractToList(contract);
        }
        foreach (IContract contract in running)
        {
            if (contract.Id == Guid.Empty)
                contract.Id = Guid.NewGuid();
            _runningContracts.Add(contract.Id, contract);
            AddContractToList(contract);
        }
    }

    public void NewGame()
    {
        NewContract(new Introduction());
    }
}
