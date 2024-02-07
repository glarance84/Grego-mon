using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, enemyMove, Busy}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playyerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScript partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    PokemonPart playerParty;
    Gregomon wildGregomon;

    public void StartBattle(PokemonPart playerParty, Gregomon wildGregomon)
    {
        this.playerParty = playerParty;
        this.wildGregomon = wildGregomon;
        StartCoroutine(SetUpBattle());
    }

    public IEnumerator SetUpBattle()
    {
        playyerUnit.Setup(playerParty.GetHealthyPokemon());
        playerHud.SetData(playyerUnit.Gregomon);
        enemyUnit.Setup(wildGregomon);
        enemyHud.SetData(enemyUnit.Gregomon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playyerUnit.Gregomon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Gregomon.Base.Name} appeared");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Choose an action");
        dialogBox.enableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.SetPartyyData(playerParty.Gregomons);
        partyScreen.gameObject.SetActive(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.enableActionSelector(false);
        dialogBox.enableDialogText(false);
        dialogBox.enableMoveSelctor(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playyerUnit.Gregomon.Moves[currentMove];
        move.PP--;
        yield return dialogBox.TypeDialog($"{playyerUnit.Gregomon.Base.Name} used {move.Base.Name}");

        playyerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();
        var damageDetails = enemyUnit.Gregomon.TakeDamage(move, playyerUnit.Gregomon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Gregomon.Base.Name} fainted");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.enemyMove;

        var move = enemyUnit.Gregomon.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Gregomon.Base.Name} used {move.Base.Name}");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playyerUnit.PlayHitAnimation();
        var damageDetails = playyerUnit.Gregomon.TakeDamage(move, playyerUnit.Gregomon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playyerUnit.Gregomon.Base.Name} fainted");
            playyerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            var nextGregomon = playerParty.GetHealthyPokemon();
            if (nextGregomon != null)
            {
                playyerUnit.Setup(nextGregomon);
                playerHud.SetData(nextGregomon);
                
                
                dialogBox.SetMoveNames(nextGregomon.Moves);

                yield return dialogBox.TypeDialog($"Go {nextGregomon.Base.Name}!");

                PlayerAction();
            }
            else
            {
                OnBattleOver(false);
            }
            
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective");

    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }

    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }    
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }
            

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //Bag
            }
            else if (currentAction == 2)
            {
                //Gregomon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKey(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKey(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playyerUnit.Gregomon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playyerUnit.Gregomon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.enableMoveSelctor(false);
            dialogBox.enableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.enableMoveSelctor(false);
            dialogBox.enableDialogText(true);
            PlayerAction();
        }
    }
}
