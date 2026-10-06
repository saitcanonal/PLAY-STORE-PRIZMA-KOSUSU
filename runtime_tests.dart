import 'dart:io';
import 'package:flutter_test/flutter_test.dart' as ft;
import 'package:test_api/src/backend/declarer.dart';
import 'package:test_api/src/backend/group.dart';
import 'package:test_api/src/backend/test.dart';
import 'package:test_api/src/backend/suite.dart';
import 'package:test_api/src/backend/suite_platform.dart';
import 'package:test_api/src/backend/runtime.dart';
import 'package:test_api/src/backend/state.dart' as ts;
import '../app/test/engine_test.dart' as engine;
import '../app/test/profile_test.dart' as profile;
import '../app/test/appearance_test.dart' as appearance;
import '../app/test/rank_recovery_test.dart' as recovery;
import '../app/test/v9_visual_test.dart' as visual;
import '../app/test/widget_test.dart' as widgets;
import '../app/test/v10_test.dart' as v10;
import '../app/test/v11_test.dart' as v11;

int count = 0, failures = 0;
Future<void> walk(Group group, Suite suite, List<Group> parents) async {
  final groups = [...parents, group];
  if (group.setUpAll != null) await run(group.setUpAll!, suite, groups);
  for (final entry in group.entries) {
    if (entry is Group) {
      await walk(entry, suite, groups);
    } else {
      await run(entry as Test, suite, groups);
    }
  }
  if (group.tearDownAll != null) await run(group.tearDownAll!, suite, groups);
}

Future<void> run(Test test, Suite suite, List<Group> groups) async {
  final live = test.load(suite, groups: groups);
  live.onError.listen((e) => stderr.writeln('FAIL ${test.name}: $e'));
  await live.run();
  if (live.state.result != ts.Result.success &&
      live.state.result != ts.Result.skipped) {
    failures++;
  } else {
    count++;
    print('PASS ${test.name}');
  }
  await live.close();
}

Future<void> main() async {
  ft.AutomatedTestWidgetsFlutterBinding();
  final declarer = Declarer();
  declarer.declare(() {
    ft.group('V11', v11.main);
    ft.group('V10', v10.main);
    ft.group('Engine', engine.main);
    ft.group('Profile', profile.main);
    ft.group('Appearance', appearance.main);
    ft.group('Recovery', recovery.main);
    ft.group('Visual', visual.main);
    ft.group('Widgets', widgets.main);
  });
  final group = declarer.build();
  final suite = Suite(group, SuitePlatform(Runtime.vm));
  await walk(group, suite, []);
  print('TOTAL $count passed, $failures failed');
  exit(failures == 0 ? 0 : 1);
}
