using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PeformMove, Busy, PartyScreen, BattleOver}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playyerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScript partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

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
        enemyUnit.Setup(wildGregomon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playyerUnit.Gregomon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Gregomon.Base.Name} appeared");

        ChooseFirstTurn();
    }

    void ChooseFirstTurn()
    {
        if (playyerUnit.Gregomon.Speed >= enemyUnit.Gregomon.Speed)
        {
            ActionSelection();
        }
        else
            StartCoroutine(EnemyMove());
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Gregomons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.enableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyyData(playerParty.Gregomons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.enableActionSelector(false);
        dialogBox.enableDialogText(false);
        dialogBox.enableMoveSelctor(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PeformMove;

        var move = playyerUnit.Gregomon.Moves[currentMove];
        yield return RunMove(playyerUnit, enemyUnit, move);
        
        if (state == BattleState.PeformMove)
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PeformMove;

        var move = enemyUnit.Gregomon.GetRandomMove();
        yield return RunMove(enemyUnit, playyerUnit, move);


        if (state == BattleState.PeformMove)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {

        bool canRunMove = sourceUnit.Gregomon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Gregomon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Gregomon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Gregomon.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        targetUnit.PlayHitAnimation();

        
        if (move.Base.Category == MoveBase.MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Gregomon, targetUnit.Gregomon);
        }
        else
        {
            var damageDetails = targetUnit.Gregomon.TakeDamage(move, sourceUnit.Gregomon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }


        if (targetUnit.Gregomon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Gregomon.Base.Name} fainted");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }

        // Statuses like burn or psn will hurt the gregomon after the turn
        sourceUnit.Gregomon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Gregomon);
        yield return sourceUnit.Hud.UpdateHP();
        if (targetUnit.Gregomon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Gregomon.Base.Name} fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Gregomon source, Gregomon target)
    {
        var effects = move.Base.Effects;

        //Stat Boosting
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveBase.MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Gregomon gregomon)
    {
        while (gregomon.StatusChanges.Count > 0)
        {
            var message = gregomon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextGregomon = playerParty.GetHealthyPokemon();
            if (nextGregomon == null)
                BattleOver(false);
            else
                OpenPartyScreen();
        }
        else
            BattleOver(true);
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
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
                MoveSelection();
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
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playyerUnit.Gregomon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playyerUnit.Gregomon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.enableMoveSelctor(false);
            dialogBox.enableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.enableMoveSelctor(false);
            dialogBox.enableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Gregomons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Gregomons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted Gregomon");
                return;
            }
            if (selectedMember == playyerUnit.Gregomon)
            {
                partyScreen.SetMessageText("You can't switch with the same Gregomon");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchGregomon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(true);
            ActionSelection();
        }

    }

    IEnumerator SwitchGregomon(Gregomon newGregomon)
    {
        bool currentGregomonfainted = true;
        if (playyerUnit.Gregomon.HP > 0)
        {
            currentGregomonfainted = false;
            yield return dialogBox.TypeDialog($"Come back {playyerUnit.Gregomon.Base.Name}");
            playyerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2);
        }

        playyerUnit.Setup(newGregomon);

        dialogBox.SetMoveNames(newGregomon.Moves);

        yield return dialogBox.TypeDialog($"Go {newGregomon.Base.Name}!");

        if (currentGregomonfainted)
            ChooseFirstTurn();
        else
            StartCoroutine(EnemyMove());

    }
}
