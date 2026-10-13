using System;
using System.Collections.Generic;
using System.Linq;
using JxModule;
using JxModule.Terminal;

namespace Jongmin
{
    public sealed class RunCommands
    {
        private readonly EventDomain _eventDomain;

        public RunCommands(EventDomain eventDomain)
        {
            _eventDomain = eventDomain;
        }

        [JxCommand("Save Status", Description = "선택한 저장 슬롯과 진행 중인 런의 체크포인트를 표시합니다.")]
        private string ShowSaveStatus()
        {
            int slotNumber = RunSaveService.SelectedSlot + 1;
            bool exists = SaveSlotSystem.Exist(RunSaveService.SelectedSlot);
            ActiveRunSaveData run = RunSaveService.ActiveRun;
            if (run == null)
            {
                return $"Slot: {slotNumber}\nSave File: {exists}\nActive Run: False";
            }

            string areaID = string.IsNullOrWhiteSpace(run.areaID) ? "-" : run.areaID;
            string eventName = run.checkpointType == ERunCheckpointType.EventEntry ? ((AreaEventType)run.eventType).ToString() : "-";
            return $"Slot: {slotNumber}\nSave File: {exists}\nActive Run: True\nRun ID: {run.runID}\nSeed: {run.seed}\nCheckpoint: {run.checkpointType}\nArea: {areaID}\nEvent: {eventName}\nArea Choices: {run.areaChoices.Count}\nDeck: {run.deck.Count}\nRun Records: {run.experienceRecords.Count}\nRandom Streams: {run.randomStreams.Count}";
        }

        [JxCommand("Save Slot List", Description = "선택할 수 있는 저장 슬롯 번호를 표시합니다.")]
        private IEnumerable<string> ListSaveSlots()
        {
            for (int slot = 0; slot < SaveSlotSystem.MaxSlotCount; slot++)
            {
                yield return (slot + 1).ToString();
            }
        }

        [JxCommand("Select Save Slot", Description = "사용할 저장 슬롯을 선택하고 해당 슬롯의 데이터를 불러옵니다.")]
        private JxCommandResult SelectSaveSlot([JxOptionValue("Save Slot List")] int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > SaveSlotSystem.MaxSlotCount)
            {
                return JxCommandResult.Failure($"슬롯 번호는 1~{SaveSlotSystem.MaxSlotCount}여야 합니다.");
            }

            RunSaveService.SelectSlot(slotNumber - 1);
            return JxCommandResult.Success($"저장 슬롯 {slotNumber}을 선택했습니다. Active Run: {RunSaveService.HasActiveRun}");
        }

        [JxCommand("Begin Run", Description = "선택한 슬롯에 새 런을 시작합니다. seed가 0이면 자동 시드를 사용합니다.")]
        private JxCommandResult BeginRun(int seed = 0)
        {
            if (RunSaveService.HasActiveRun)
            {
                return JxCommandResult.Failure("진행 중인 런이 있습니다. 먼저 End Run 명령으로 종료하세요.");
            }

            RunSaveService.BeginRun(seed == 0 ? null : seed);
            return JxCommandResult.Success($"새 런을 시작했습니다. Seed: {RunSaveService.ActiveRun.seed}");
        }

        [JxCommand("Run End Reason List", Description = "End Run에서 사용할 수 있는 종료 사유를 표시합니다.")]
        private IEnumerable<string> ListRunEndReasons()
        {
            return Enum.GetNames(typeof(ERunEndReason));
        }

        [JxCommand("End Run", Description = "현재 런을 지정한 사유로 종료하고 영구 진행도를 저장합니다.")]
        private JxCommandResult EndRun([JxOptionValue("Run End Reason List")] ERunEndReason reason)
        {
            if (!RunSaveService.HasActiveRun)
            {
                return JxCommandResult.Failure("진행 중인 런이 없습니다.");
            }

            RunSaveService.EndRun(reason);
            return JxCommandResult.Success($"런을 {reason} 사유로 종료했습니다.");
        }

        [JxCommand("Save Slot Progress", Description = "런 밖에서 변경된 유물과 누적 경험 기록을 즉시 저장합니다.")]
        private JxCommandResult SaveSlotProgress()
        {
            if (RunSaveService.HasActiveRun)
            {
                return JxCommandResult.Failure("진행 중인 런에서는 지역 진입 저장을 사용해야 합니다.");
            }

            RunSaveService.SaveSlotProgress();
            return JxCommandResult.Success($"슬롯 {RunSaveService.SelectedSlot + 1}의 영구 진행도를 저장했습니다.");
        }

        [JxCommand("Experience Scope List", Description = "경험 기록 조회 범위를 표시합니다.")]
        private IEnumerable<string> ListExperienceScopes()
        {
            return Enum.GetNames(typeof(EExperienceRecordScope));
        }

        [JxCommand("Experience Type List", Description = "경험 기록 종류를 표시합니다.")]
        private IEnumerable<string> ListExperienceTypes()
        {
            return Enum.GetNames(typeof(EExperienceRecordType));
        }

        [JxCommand("Experience Get", Description = "범위, 종류, 대상, 세부 대상으로 경험 기록 값을 조회합니다.")]
        private JxCommandResult GetExperience([JxOptionValue("Experience Scope List")] EExperienceRecordScope scope, [JxOptionValue("Experience Type List")] EExperienceRecordType type, string targetID, string detailID = "")
        {
            IExperienceRecord records = scope == EExperienceRecordScope.Slot ? _eventDomain?.SlotProgress?.Experience : _eventDomain?.RunProgress?.Experience;
            if (records == null)
            {
                return JxCommandResult.Failure("EventDomain이 아직 초기화되지 않았습니다.");
            }

            int value = records.Get(type, targetID, detailID);
            string detail = string.IsNullOrWhiteSpace(detailID) ? "-" : detailID;
            return JxCommandResult.Success($"Scope: {scope}\nType: {type}\nTarget: {targetID}\nDetail: {detail}\nValue: {value}");
        }

        [JxCommand("Experience List", Description = "선택한 범위에 기록된 모든 경험 값을 표시합니다.")]
        private IEnumerable<string> ListExperience([JxOptionValue("Experience Scope List")] EExperienceRecordScope scope)
        {
            IEnumerable<ExperienceRecordData> records = scope == EExperienceRecordScope.Slot ? _eventDomain?.CaptureSlotProgress()?.experienceRecords : _eventDomain?.CaptureRunProgress();
            if (records == null)
            {
                return new[] { "EventDomain이 아직 초기화되지 않았습니다." };
            }

            string[] lines = records.OrderBy(record => record.type).ThenBy(record => record.targetID).ThenBy(record => record.detailID).Select(FormatExperienceRecord).ToArray();
            return lines.Length > 0 ? lines : new[] { $"{scope} 범위에 기록된 경험이 없습니다." };
        }

        [JxCommand("Run Random State", Description = "용도별 무작위 스트림의 소비 횟수를 표시합니다.")]
        private IEnumerable<string> ShowRandomState()
        {
            List<RandomStreamSaveData> streams = RunRandom.Capture();
            if (streams.Count == 0)
            {
                return new[] { "사용된 무작위 스트림이 없습니다." };
            }

            return streams.OrderBy(stream => stream.streamID).Select(stream => $"{stream.streamID}: {stream.consumed}");
        }

        private static string FormatExperienceRecord(ExperienceRecordData record)
        {
            string target = string.IsNullOrWhiteSpace(record.targetID) ? "-" : record.targetID;
            string detail = string.IsNullOrWhiteSpace(record.detailID) ? "-" : record.detailID;
            return $"{record.type} | {target} | {detail} = {record.value}";
        }
    }
}
